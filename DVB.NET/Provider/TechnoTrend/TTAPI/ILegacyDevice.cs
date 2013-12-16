using System;
using JMS.DVB;
using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.TechnoTrend
{
    /// <summary>
    /// Abstraction of the DVB hardware access.
    /// </summary>
    public interface ILegacyDevice : IDisposable
    {
        /// <summary>
        /// Stop all PID filters.
        /// </summary>
        void StopFilters();

        /// <summary>
        /// Wählt eine Quellgruppe an.
        /// </summary>
        /// <param name="group">Die Quellgruppe.</param>
        /// <param name="location">Der Ursprung zur Quellgruppe.</param>
        void Tune( SourceGroup group, GroupLocation location );

        /// <summary>
        /// Set the video and primary audio PID for display and single
        /// channel recording.
        /// </summary>
        /// <param name="videoPID">The video signal to show.</param>
        /// <param name="audioPID">The audio signal to activate.</param>
        void SetVideoAudio( ushort videoPID, ushort audioPID );

        /// <summary>
        /// Start a section filter.
        /// </summary>
        /// <param name="pid">PID to filter upon.</param>
        /// <param name="callback">Method to call when new data is available.</param>
        /// <param name="filterData">Filter data for pre-selection.</param>
        /// <param name="filterMask">Masks those bits in the filter data for pre-selection
        /// which are relevant for comparision.</param>
        void StartSectionFilter( ushort pid, Action<byte[]> callback, byte[] filterData, byte[] filterMask );

        /// <summary>
        /// Prepare filtering a DVB stream.
        /// </summary>
        /// <remarks>
        /// Use <see cref="StartFilter"/> to start filtering.
        /// </remarks>
        /// <param name="pid">PID to filter upon.</param>
        /// <param name="video">Set if a video stream is used.</param>
        /// <param name="smallBuffer">Unset if the largest possible buffer should be used.</param>
        /// <param name="callback">Method to call when new data is available.</param>
        void RegisterPipingFilter( ushort pid, bool video, bool smallBuffer, Action<byte[]> callback );

        /// <summary>
        /// Start filtering a DVB stream.
        /// </summary>
        /// <param name="pid">PID on which the filter runs.</param>
        void StartFilter( ushort pid );

        /// <summary>
        /// Suspend filtering a DVB stream.
        /// </summary>
        /// <param name="pid">PID on which the filter runs.</param>
        void StopFilter( ushort pid );

        /// <summary>
        /// Activate decrypting the indicated station.
        /// </summary>
        /// <param name="station">Some station.</param>
        void Decrypt( ushort? station );

        /// <summary>
        /// Called after a wakeup from hibernation prior to the first tune request.
        /// </summary>
        void WakeUp();

        /// <summary>
        /// Report the current status of the signal.
        /// </summary>
        SignalStatus SignalStatus { get; }
    }
}
