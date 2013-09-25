using System;
using System.Windows.Forms;
using System.Configuration;
using System.Runtime.InteropServices;

using JMS.DVB.DeviceAccess;
using JMS.DVB.DirectShow.Interfaces;
using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.DirectShow
{
    /// <summary>
    /// Vereinfacht den Zugriff auf den Darstellungsfilter.
    /// </summary>
    internal class VMR : TypedComIdentity<IBaseFilter>
    {
        /// <summary>
        /// Erzeugt einen neuen Darstellungsfilter.
        /// </summary>
        /// <param name="instance">Die COM Schnittstelle auf den Filter.</param>
        private VMR( object instance )
            : base( instance )
        {
            // Unmap
            using (var vmr = MarshalToManaged())
            {
                // Change type
                var config = (IVMRFilterConfig) vmr.Object;

                // Set all
                ((IVMRAspectRatioControl9) vmr.Object).SetAspectRatioMode( AspectRatioModes.LetterBox );
                config.SetRenderingMode( VMRModes.Windowed );
                config.SetNumberOfStreams( 1 );
            }
        }

        /// <summary>
        /// Erzeugt einen neuen Filter.
        /// </summary>
        /// <returns>Der gewünschte Filter.</returns>
        public static VMR Create()
        {
            // Create
            var instance = Marshal.BindToMoniker( VMRMoniker );
            try
            {
                // Report
                return new VMR( instance );
            }
            finally
            {
                // Cleanup
                BDAEnvironment.Release( ref instance );
            }
        }

        /// <summary>
        /// Meldet den eindeutigen Namen des zu verwendenden Filters.
        /// </summary>
        private static string VMRMoniker
        {
            get
            {
                // Check for customization
                string vmr = ConfigurationManager.AppSettings["VMR"];
                if (!string.IsNullOrEmpty( vmr ))
                    return vmr;
                else
                    return @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{51B4ABF3-748F-4E3B-A276-C828330E926A}";
            }
        }

        /// <summary>
        /// Meldet das Windows Wenster an, in dem das Bild darzustellen ist.
        /// </summary>
        public Control ClippingWindow
        {
            set
            {
                // Check mode
                using (var vmr = MarshalToManaged())
                {
                    // Change type
                    var video = (IVideoWindow) vmr.Object;

                    // Attach to the control
                    video.Owner = value.Handle;
                    video.WindowStyle = 0x44000000;
                }
            }
        }

        /// <summary>
        /// Meldet das aktuelle Seitenverhältnis des Bildes.
        /// </summary>
        public double? CurrentAspectRatio
        {
            get
            {
                // Be safe
                try
                {
                    // Unmap
                    using (var vmr = MarshalToManaged())
                    {
                        // Load parts
                        var vid = vmr.Object as IBasicVideo;
                        if (vid == null)
                            return null;
                        var wid = vid.VideoWidth;
                        if (wid <= 0)
                            return null;
                        var hei = vid.VideoHeight;
                        if (hei <= 0)
                            return null;

                        // Report current ratio
                        return wid / hei;
                    }
                }
                catch
                {
                    // Ignore any error
                    return null;
                }
            }
        }

        /// <summary>
        /// Meldet eine veränderte Größe des Darstellungsfensters.
        /// </summary>
        /// <param name="holder">Das zu verwendene Windows Fenster.</param>
        public void AdjustSize( Control holder )
        {
            // Check mode
            using (var vmr = MarshalToManaged())
                ((IVideoWindow) vmr.Object).SetWindowPosition( 0, 0, holder.Width, holder.Height );
        }
    }
}
