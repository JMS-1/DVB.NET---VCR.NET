using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;


namespace JMS.DVB.DirectShow.RawDevices
{
    /// <summary>
    /// Erstellt eine einfache Oberfläche zur Zuordnung der Tasten der Fernbedienung zu den
    /// <see cref="InputKey"/> Elementen.
    /// </summary>
    public partial class RemoteControlLearningGrid : UserControl
    {
        /// <summary>
        /// Nimmt alle Nachrichten entgegen.
        /// </summary>
        private class _Sink : System.Windows.Forms.Form
        {
            /// <summary>
            /// Die zugehörige Steuereinheit.
            /// </summary>
            public RawInputSink RawDevice { get; set; }

            /// <summary>
            /// Erzeugt ein neues Fenster.
            /// </summary>
            public _Sink()
            {
                // Configure
                Text = "[RC Receiver]";
            }

            /// <summary>
            /// Bearbeitet eine Windows Meldung über eine Benutzereingabe.
            /// </summary>
            /// <param name="m">Die zu bearbeitende Meldung</param>
            protected override void WndProc( ref System.Windows.Forms.Message m )
            {
                // Pre process
                if (RawDevice != null)
                    if (RawDevice.ProcessMessage( ref m ))
                        return;

                // Forward
                base.WndProc( ref m );
            }
        }

        /// <summary>
        /// Nimmt die Anfrage der Fernbedienung entgegen.
        /// </summary>
        private RawInputSink m_RawDevice;

        /// <summary>
        /// Erzeugt eine neue Oberfläche.
        /// </summary>
        public RemoteControlLearningGrid()
        {
            // Load designer stuff
            InitializeComponent();
        }

        /// <summary>
        /// Wird aufgerufen, wenn dieses Oberflächenelement geladen wurde.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void UserControl_Loaded( object sender, EventArgs e )
        {
            // Connect
            m_RawDevice = RawInputSink.Create();

            // Attach to notification
            m_RawDevice.SetReceiver( Process );
        }

        /// <summary>
        /// Wird aufgerufen, wenn das Oberflächenelement entfernt wurde.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void UserControl_Unloaded( object sender, RoutedEventArgs e )
        {
            // Cleanup
            using (m_RawDevice)
                m_RawDevice = null;
        }

        /// <summary>
        /// Bearbeitet eine Eingabe der Fernbedienung.
        /// </summary>
        /// <param name="code">Der Eingabecode.</param>
        private void Process( MappingItem code )
        {
            // Load the storyboard
            var animation = ReceiverFlag.FindResource( "Receiver" ) as Storyboard;
            if (animation != null)
                animation.Begin();

            // Attach to our helper
            var context = DataContext as LearnContext;
            if (context != null)
                context.RegisterSequence( code );
        }
    }
}
