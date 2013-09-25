using System;
using System.Reflection;


namespace JMS.DVB.DeviceAccess.Topology
{
    /// <summary>
    /// Enthält die eindeutigen Kennungen für die Funktionen von Hardwaregeräten innerhalb von
    /// BDA DVB Komponenten.
    /// </summary>
    public static class NodeFunctions
    {
        /// <summary>
        /// Eindeutige Kennung für den Tuner.
        /// </summary>
        public static readonly Guid RFTuner = new Guid( "71985f4c-1ca1-11d3-9cc8-00c04f7971e0" );

        /// <summary>
        /// Eindeutige Kennung für einen Demodulator (Analog).
        /// </summary>
        public static readonly Guid AnalogDemodulator = new Guid( "634db199-27dd-46b8-acfb-ecc98e61a2ad" );

        /// <summary>
        /// Eindeutige Kennung für einen Demodulator (QAM).
        /// </summary>
        public static readonly Guid QAMDemodulator = new Guid( "71985f4d-1ca1-11d3-9cc8-00c04f7971e0" );

        /// <summary>
        /// Eindeutige Kennung für einen Demodulator (QPSK).
        /// </summary>
        public static readonly Guid QPSKDemodulator = new Guid( "6390c905-27c1-4d67-bdb7-77c50d079300" );

        /// <summary>
        /// Eindeutige Kennung für einen Demodulator (8VSB).
        /// </summary>
        public static readonly Guid VSB8Demodulator = new Guid( "71985f4f-1ca1-11d3-9cc8-00c04f7971e0" );

        /// <summary>
        /// Eindeutige Kennung für einen Demodulator (COFDM).
        /// </summary>
        public static readonly Guid COFDMDemodulator = new Guid( "2dac6e05-edbe-4b9c-b387-1b6fad7d6495" );

        /// <summary>
        /// Eindeutige Kennung für einen Demodulator (8PSK).
        /// </summary>
        public static readonly Guid PSK8Demodulator = new Guid( "e957a0e7-dd98-4a3c-810b-3525157ab62e" );

        /// <summary>
        /// [Don't know]
        /// </summary>
        public static readonly Guid OpenCablePOD = new Guid( "345812a0-fb7c-4790-aa7e-b1db88ac19c9" );

        /// <summary>
        /// Eindeutige Kennung für eine CI Ansteuerung.
        /// </summary>
        public static readonly Guid CommonCAPOD = new Guid( "d83ef8fc-f3b8-45ab-8b71-ecf7c339deb4" );

        /// <summary>
        /// Eindeutige Kennung für einen Datenstromfilter (PID).
        /// </summary>

        public static readonly Guid PIDFilter = new Guid( "f5412789-b0a0-44e1-ae4f-ee999b1b7fbe" );
        /// <summary>
        /// [Don't know]
        /// </summary>

        public static readonly Guid IPSink = new Guid( "71985f4e-1ca1-11d3-9cc8-00c04f7971e0" );

        /// <summary>
        /// Eindeutige Kennung für einen Video Encoder.
        /// </summary>
        public static readonly Guid VideoEncoder = new Guid( "d98429e3-65c9-4ac4-93aa-766782833b7a" );

        /// <summary>
        /// Ermittelt zu einer Funktion einen Kurznamen.
        /// </summary>
        /// <param name="function">Die eindeutige Kennung der Funktion.</param>
        /// <returns>Der zugehörige Kurzname.</returns>
        public static string GetName( Guid function )
        {
            // Uses reflection
            foreach (FieldInfo field in typeof( NodeFunctions ).GetFields( BindingFlags.Public | BindingFlags.Static ))
                if (typeof( Guid ) == field.FieldType)
                {
                    // Load the value
                    Guid value = (Guid) field.GetValue( null );

                    // Compare
                    if (value == function) return field.Name;
                }

            // Not found
            return function.ToString( "B" );
        }
    }
}
