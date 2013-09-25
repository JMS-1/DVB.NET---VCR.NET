using System;

using JMS.DVB.DirectShow.Interfaces;


namespace JMS.DVB.DirectShow
{
    /// <summary>
    /// Beschreibt die Bildparameter.
    /// </summary>
    public class PictureParameters
    {
        /// <summary>
        /// Beschreibt einen einzelnen Parameter.
        /// </summary>
        public class ParameterSet
        {
            /// <summary>
            /// Erlaubter Minimalwert.
            /// </summary>
            public readonly float Minimum;

            /// <summary>
            /// Erlaubter Maximalwert.
            /// </summary>
            public readonly float Maximum;

            /// <summary>
            /// Der bevorzugte Wert.
            /// </summary>
            public readonly float Default;

            /// <summary>
            /// Die Granularität der Einstellungen.
            /// </summary>
            public readonly float Step;

            /// <summary>
            /// Der aktuelle Wert.
            /// </summary>
            public float Value;

            /// <summary>
            /// Erzeugt eine neue Beschreibung.
            /// </summary>
            /// <param name="value">Der aktuelle Wert.</param>
            /// <param name="control">Die zugehörige Steuereinheit.</param>
            /// <param name="property">Die gewünschte Einstellung.</param>
            internal ParameterSet( float value, IVMRMixerControl control, VMRProcAmpControlFlags property )
            {
                // Create helper
                var range = VMRProcAmpControlRange.Create( property );

                // Read
                control.GetProcAmpControlRange( 0, ref range );

                // Load all
                Default = range.DefaultValue;
                Minimum = range.MinValue;
                Maximum = range.MaxValue;
                Step = range.StepSize;
                Value = value;
            }
        }

        /// <summary>
        /// Die Helligkeit.
        /// </summary>
        public readonly ParameterSet Brightness;

        /// <summary>
        /// Der Kontrast.
        /// </summary>
        public readonly ParameterSet Contrast;

        /// <summary>
        /// Der Farbton.
        /// </summary>
        public readonly ParameterSet Hue;

        /// <summary>
        /// Die Sättigung.
        /// </summary>
        public readonly ParameterSet Saturation;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="vmr9">Der zu verwendende Darstellungsfilter.</param>
        internal PictureParameters( VMR vmr9 )
        {
            // Unmap
            using (var vmr = vmr9.MarshalToManaged())
            {
                // Attach to interface
                var mixer = (IVMRMixerControl) vmr.Object;

                // Current values
                var values = VMRProcAmpControl.Create( VMRProcAmpControlFlags.Brightness | VMRProcAmpControlFlags.Contrast | VMRProcAmpControlFlags.Hue | VMRProcAmpControlFlags.Saturation );

                // Load
                mixer.GetProcAmpControl( 0, ref values );

                // Create
                Saturation = new ParameterSet( values.Saturation, mixer, VMRProcAmpControlFlags.Saturation );
                Brightness = new ParameterSet( values.Brightness, mixer, VMRProcAmpControlFlags.Brightness );
                Contrast = new ParameterSet( values.Contrast, mixer, VMRProcAmpControlFlags.Contrast );
                Hue = new ParameterSet( values.Hue, mixer, VMRProcAmpControlFlags.Hue );
            }
        }

        /// <summary>
        /// Aktualisiert die Einstellungen.
        /// </summary>
        /// <param name="vmr9">Der zu verwendende Darstellungsfilter.</param>
        internal void Update( VMR vmr9 )
        {
            // Be safe
            try
            {
                // Unmap
                using (var vmr = vmr9.MarshalToManaged())
                {
                    // Attach to interface
                    var mixer = (IVMRMixerControl) vmr.Object;

                    // Current values
                    var values = VMRProcAmpControl.Create( VMRProcAmpControlFlags.Brightness | VMRProcAmpControlFlags.Contrast | VMRProcAmpControlFlags.Hue | VMRProcAmpControlFlags.Saturation );

                    // Fill
                    values.Brightness = Brightness.Value;
                    values.Saturation = Saturation.Value;
                    values.Contrast = Contrast.Value;
                    values.Hue = Hue.Value;

                    // Send update
                    mixer.SetProcAmpControl( 0, ref values );
                }
            }
            catch
            {
                // Ignore any error
            }
        }
    }
}
