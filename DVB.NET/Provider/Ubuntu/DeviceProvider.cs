using JMS.DVB.DeviceAccess.Interfaces;
using JMS.TechnoTrend;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JMS.DVB.Provider.Ubuntu
{
    public class DeviceProvider : ILegacyDevice
    {
        private readonly int m_adapter;

        private readonly int m_frontend;

        private readonly string m_server;

        private readonly int m_port;

        private TcpClient m_connection;

        private byte[] m_input = new byte[Marshal.SizeOf<FrontendResponse>()];

        private GCHandle m_inPtr;

        private Dictionary<ushort, Action<byte[]>> m_filters = new Dictionary<ushort, Action<byte[]>>();

        private readonly object m_lock = new object();

        public DeviceProvider(Hashtable args)
        {            
            m_server = (string)args["Adapter.Server"];
            m_port = ArgumentToNumber(args["Adapter.Port"], 29713);

            m_adapter = ArgumentToNumber(args["Adapter.Index"]);
            m_frontend = ArgumentToNumber(args["Adapter.Frontend"]);

            m_inPtr = GCHandle.Alloc(m_input, GCHandleType.Pinned);
        }

        private static int ArgumentToNumber(object arg, int fallback = 0)
        {
            if (int.TryParse((string)arg, out int number))
                return number;

            return fallback;
        }

        private void SendRequest<TData>(FrontendRequestType type, TData data)
        {
            Open();

            var size = 4 + Marshal.SizeOf(data);
            var buf = new byte[size];
            var bufptr = GCHandle.Alloc(buf, GCHandleType.Pinned);

            try
            {
                Marshal.WriteInt32(bufptr.AddrOfPinnedObject() + 0, (Int32)type);
                Marshal.StructureToPtr(data, bufptr.AddrOfPinnedObject() + 4, true);
            }
            finally
            {
                bufptr.Free();
            }

            SafeWrite(buf);
        }

        private void SafeWrite(byte[] buf)
        {
            try
            {
                m_connection.GetStream().Write(buf, 0, buf.Length);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to send request: {0}", e);
            }
        }

        private void SendRequest(FrontendRequestType type, UInt16 data)
        {
            Open();

            var buf = new byte[6];
            var bufptr = GCHandle.Alloc(buf, GCHandleType.Pinned);

            try
            {
                Marshal.WriteInt32(bufptr.AddrOfPinnedObject() + 0, (Int32)type);
                Marshal.WriteInt16(bufptr.AddrOfPinnedObject() + 4, (Int16)data);
            }
            finally
            {
                bufptr.Free();
            }

            SafeWrite(buf);
        }

        private void SendRequest(FrontendRequestType type)
        {
            Open();

            var buf = new byte[4];
            var bufptr = GCHandle.Alloc(buf, GCHandleType.Pinned);

            try
            {
                Marshal.WriteInt32(bufptr.AddrOfPinnedObject() + 0, (Int32)type);
            }
            finally
            {
                bufptr.Free();
            }

            SafeWrite(buf);
        }


        private bool ReadBuffer(byte[] buffer)
        {
            for(var offset = 0; offset < buffer.Length; )
            {
                if(m_connection== null)
                {
                    return false;
                }

                try
                {
                    var read = m_connection.GetStream().Read(buffer, offset, buffer.Length - offset);

                    if (read <= 0)
                    {
                        return false;
                    }

                    offset += read;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        private void StartReader(object state)
        {            
            for(; ; )
            {
                if (!ReadBuffer(m_input))
                {
                    return;
                }

                var response = Marshal.PtrToStructure<FrontendResponse>(m_inPtr.AddrOfPinnedObject());

                switch (response.type)
                {
                    case FrontendResponseType.section:
                    case FrontendResponseType.stream:
                    case FrontendResponseType.signal:
                        break;
                    default:
                        return;
                }

                if(response.len < 0 || response.len > 10 * 1024 * 1024)
                {
                    return;
                }

                if (response.pid > ushort.MaxValue)
                {
                    return;
                }

                var buf = new byte[response.len];

                if (!ReadBuffer(buf))
                {
                    return;
                }

                switch (response.type)
                {
                    case FrontendResponseType.section:
                    case FrontendResponseType.stream:
                        Action<byte[]> callback;

                        lock (m_lock) {
                            if(!m_filters.TryGetValue(response.pid, out callback))
                            {
                                callback = null;
                            }
                        }
                        
                        callback?.Invoke(buf);

                        break;
                    case FrontendResponseType.signal:
                        var bufPtr = GCHandle.Alloc(buf, GCHandleType.Pinned);

                        try
                        {
                            var signal = Marshal.PtrToStructure<SignalInformation>(bufPtr.AddrOfPinnedObject());

                            if ((signal.status & FeStatus.FE_HAS_LOCK) == FeStatus.FE_HAS_LOCK)
                            {
                                SignalStatus = new SignalStatus(true, signal.snr / 10.0, (signal.strength * 1.0) / UInt16.MaxValue);
                            }
                            else
                            {
                                SignalStatus = new SignalStatus(false, 0, 0);
                            }
                        }
                        finally
                        {
                            bufPtr.Free();
                        }

                        break;
                }
            }
        }

        private void Open()
        {
            if (m_connection != null)
            {
                return;
            }

            m_connection = new TcpClient{ ReceiveBufferSize = 10 * 1024 * 1024 };

            try
            {
                m_connection.Connect(m_server, m_port);

                ThreadPool.QueueUserWorkItem(StartReader);

                SendRequest(FrontendRequestType.connect_adapter, new ConnectRequest { adapter = m_adapter, frontend = m_frontend });
            }
            catch (Exception)
            {
                Close();

                throw;
            }
        }

        private void Close()
        {
            if (m_connection == null)
            {
                return;
            }

            using (m_connection)
            {
                try
                {
                    m_connection.Close();
                }
                finally
                {
                    m_connection = null;

                }
            }
        }

        public void StopFilters()
        {
            lock (m_lock)
            {
                m_filters.Clear();
            }

            if (m_connection != null)
              {
                SendRequest(FrontendRequestType.del_all_filters);
            }
        }

        private static DiSEqCModes ConvertDiSEqC(DiSEqCLocations location)
        {
            switch (location)
            {
                case DiSEqCLocations.BurstOff:
                    return DiSEqCModes.burst_off;
                case DiSEqCLocations.BurstOn:
                    return DiSEqCModes.burst_on;
                case DiSEqCLocations.DiSEqC1:
                    return DiSEqCModes.diseqc1;
                case DiSEqCLocations.DiSEqC2:
                    return DiSEqCModes.diseqc2;
                case DiSEqCLocations.DiSEqC3:
                    return DiSEqCModes.diseqc3;
                case DiSEqCLocations.DiSEqC4:
                    return DiSEqCModes.diseqc4;
                default:
                    return DiSEqCModes.none;
            }
        }

        private static FeModulation ConvertModulation(SatelliteModulations modulation)
        {
            switch (modulation)
            {
                case SatelliteModulations.Auto:
                    return FeModulation.QAM_AUTO;
                case SatelliteModulations.PSK8:
                    return FeModulation.PSK_8;
                case SatelliteModulations.QAM16:
                    return FeModulation.QAM_16;
                default:
                    return FeModulation.QPSK;
            }
        }

        private static FeCodeRate ConvertCodeRate(InnerForwardErrorCorrectionModes modulation)
        {
            switch (modulation)
            {
                case InnerForwardErrorCorrectionModes.Conv1_2:
                    return FeCodeRate.FEC_1_2;
                case InnerForwardErrorCorrectionModes.Conv2_3:
                    return FeCodeRate.FEC_2_3;
                case InnerForwardErrorCorrectionModes.Conv3_4:
                    return FeCodeRate.FEC_3_4;
                case InnerForwardErrorCorrectionModes.Conv3_5:
                    return FeCodeRate.FEC_3_5;
                case InnerForwardErrorCorrectionModes.Conv4_5:
                    return FeCodeRate.FEC_4_5;
                case InnerForwardErrorCorrectionModes.Conv5_6:
                    return FeCodeRate.FEC_5_6;
                case InnerForwardErrorCorrectionModes.Conv7_8:
                    return FeCodeRate.FEC_7_8;
                case InnerForwardErrorCorrectionModes.Conv8_9:
                    return FeCodeRate.FEC_8_9;
                case InnerForwardErrorCorrectionModes.Conv9_10:
                    return FeCodeRate.FEC_9_10;
                case InnerForwardErrorCorrectionModes.NoConv:
                    return FeCodeRate.FEC_NONE;
                default:
                    return FeCodeRate.FEC_AUTO;
            }
        }

        private static FeRolloff ConvertRolloff(S2RollOffs modulation)
        {
            switch (modulation)
            {
                case S2RollOffs.Alpha20:
                    return FeRolloff.ROLLOFF_20;
                case S2RollOffs.Alpha25:
                    return FeRolloff.ROLLOFF_25;
                case S2RollOffs.Alpha35:
                    return FeRolloff.ROLLOFF_35;
                default:
                    return FeRolloff.ROLLOFF_AUTO;
            }
        }

        public void Tune(SourceGroup group, GroupLocation location)
        {
            var satGroup = group as SatelliteGroup;

            if (satGroup == null)
            {
                return;
            }

            var satLocation = location as SatelliteLocation;

            if(satLocation == null)
            {
                return;
            }

            var tune = new SatelliteTune
            {
                frequency = satGroup.Frequency,
                horizontal = satGroup.Polarization == Polarizations.Horizontal,
                innerFEC = ConvertCodeRate(satGroup.InnerFEC),
                lnb1 = satLocation.Frequency1,
                lnb2 = satLocation.Frequency2,
                lnbMode = ConvertDiSEqC(satLocation.LNB),
                lnbPower = satLocation.UsePower,
                lnbSwitch = satLocation.SwitchFrequency,
                modulation = ConvertModulation(satGroup.Modulation),
                rolloff = ConvertRolloff(satGroup.RollOff),
                s2 = satGroup.UsesS2Modulation,
                symbolrate = satGroup.SymbolRate,
            };

            SendRequest(FrontendRequestType.tune, tune);
        }

        public void SetVideoAudio(ushort videoPID, ushort audioPID)
        {
            Open();
        }

        public void StartSectionFilter(ushort pid, Action<byte[]> callback, byte[] filterData, byte[] filterMask)
        {
            lock (m_lock)
            {
                m_filters[pid] = callback;
            }

            SendRequest(FrontendRequestType.add_section_filter, pid);
        }

        public void RegisterPipingFilter(ushort pid, bool video, bool smallBuffer, Action<byte[]> callback)
        {
            lock (m_lock)
            {
                m_filters[pid] = callback;
            }

            SendRequest(FrontendRequestType.add_stream_filter, pid);
        }

        public void StartFilter(ushort pid)
        {
        }

        public void StopFilter(ushort pid)
        {
            lock (m_lock)
            {
                m_filters.Remove(pid);
            }

            if (m_connection != null)
            {
                SendRequest(FrontendRequestType.del_filter, pid);
            }
        }

        public void Decrypt(ushort? station)
        {
        }

        public override string ToString()
        {
            return string.Format("Ubuntu DVB Proxy #{0}.{1} on {2}:{3}", m_adapter, m_frontend, m_server, m_port);
        }

        public void WakeUp()
        {
        }

        public SignalStatus SignalStatus { get; private set; } = new SignalStatus(false, 0, 0);

        public virtual void Dispose()
        {
            StopFilters();

            Close();

            if (m_inPtr.IsAllocated)
            {
                m_inPtr.Free();
            }
        }
    }

}
