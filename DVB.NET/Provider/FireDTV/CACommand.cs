using System;
using System.Runtime.InteropServices;

using JMS.DVB.EPG.Tables;


namespace JMS.DVB.Provider.FireDTV
{
    /// <summary>
    /// Struktur zur Übermittelung von Befehlen an das CI.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    internal struct CACommand
    {
        /// <summary>
        /// Maximale Anzahl von Befehlsdatenbytes.
        /// </summary>
        private const int MaxDataLength = 2 + 1024;

        /// <summary>
        /// Laufende Nummer des Slots.
        /// </summary>
        private byte Slot;

        /// <summary>
        /// Befehlsart.
        /// </summary>
        private byte Tag;

        /// <summary>
        /// Füllbytes zur Anpassung an die C++ Struktur.
        /// </summary>
        private UInt16 _Padding;

        /// <summary>
        /// Gesetzt, wenn weitere Daten folgen.
        /// </summary>            
        private UInt32 More;

        /// <summary>
        /// Länge von Befehlscodes und Daten.
        /// </summary>
        private UInt16 Length;

        /// <summary>
        /// Optionale Daten zum Befehl.
        /// </summary>
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = MaxDataLength )]
        private byte[] CommandData;

        /// <summary>
        /// Erzeugt eine Kopie.
        /// </summary>
        /// <returns>Eine exakte Kopie.</returns>
        public CACommand Clone()
        {
            // Create clone
            CACommand clone = new CACommand();

            // Fill
            clone.CommandData = (null == CommandData) ? null : (byte[]) CommandData.Clone();
            clone._Padding = _Padding;
            clone.Length = Length;
            clone.More = More;
            clone.Slot = Slot;
            clone.Tag = Tag;

            // Report
            return clone;
        }

        /// <summary>
        /// Erzeugt einen Befehlssequenz zur Deaktivierung des CI.
        /// </summary>
        /// <returns>Eine neue Befehlssequenz.</returns>
        public static CACommand CreateReset()
        {
            // Create
            CACommand command = new CACommand();

            // Fill command
            command.Tag = (byte) CACommandTags.Reset;
            command.CommandData = new byte[MaxDataLength];

            // Fill data
            command.CommandData[0] = (byte) CACommandTags.Reset;
            command.Length = 1;

            // Report
            return command;
        }

        /// <summary>
        /// Aktiviert die Entschlüsselung eines Senders.
        /// </summary>
        /// <param name="pmt">Daten zum Sender.</param>
        /// <returns>Die neue Befehlssequenz.</returns>
        public static CACommand CreateDecrypt( PMT pmt )
        {
            // Create helper
            byte[] table = pmt.Section.CreateSITable();

            // Create
            CACommand command = new CACommand();

            // Fill command
            command.Tag = (byte) CACommandTags.Decrypt;
            command.CommandData = new byte[MaxDataLength];

            // Fill data
            command.CommandData[0] = (byte) CAListTypes.One;
            command.CommandData[1] = (byte) PMTCommands.Descramble;

            // Calculate the length - we are in trouble if PMT is too long!
            command.Length = (ushort) (2 + Math.Min( command.CommandData.Length - 2, table.Length - 1 ));

            // Copy over and get rid of the offset byte
            Array.Copy( table, 1, command.CommandData, 2, command.Length - 2 );

            // Report
            return command;
        }

        /// <summary>
        /// Deaktiviert die Entschlüsselung.
        /// </summary>
        /// <returns>Die neue Befehlssequenz.</returns>
        public static CACommand StopDecryption()
        {
            // Create
            CACommand command = new CACommand();

            // Fill command
            command.Tag = (byte) CACommandTags.Decrypt;
            command.CommandData = new byte[MaxDataLength];

            // Fill data
            command.Length = 2;
            command.CommandData[0] = (byte) CAListTypes.One;
            command.CommandData[1] = (byte) PMTCommands.NotSelected;

            // Report
            return command;
        }
    }
}
