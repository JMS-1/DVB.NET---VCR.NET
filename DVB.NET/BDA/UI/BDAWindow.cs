using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using JMS.DVB.DirectShow.Interfaces;


namespace JMS.DVB.DirectShow.UI
{
    /// <summary>
    /// Fenster zur Darstellung von Bild und Ton.
    /// </summary>
    public partial class BDAWindow : UserControl
    {
        /// <summary>
        /// Signatur einer Methode zur Entgegennahme von Tasten.
        /// </summary>
        /// <param name="key">Die betätigte Taste.</param>
        public delegate void KeyProcessor( Keys key );

        /// <summary>
        /// Beschreibt die Behandlung von Tasten.
        /// </summary>
        [Browsable( false )]
        public Dictionary<Keys, KeyProcessor> KeyProcessors = new Dictionary<Keys, KeyProcessor>();

        /// <summary>
        /// Wird ausgelöst, bevor sich die Vollbilddarstellung ändert.
        /// </summary>
        public event CancelEventHandler FullScreenChanging;

        /// <summary>
        /// Wird ausgelöst, nachdem sich die Vollbilddarstellung verändert hat.
        /// </summary>
        public event EventHandler FullScreenChanged;

        /// <summary>
        /// Wird ausgelöst, nachdem die Lautstärke verändert wurde.
        /// </summary>
        public event EventHandler VolumeChanged;

        /// <summary>
        /// Der zugehörige DirectShow Graph.
        /// </summary>
        public DisplayGraph Graph { get; private set; }

        /// <summary>
        /// Die Komponente zum Zugriff auf den Rohdatenstrom.
        /// </summary>
        private AccessModule m_Accessor = null;

        /// <summary>
        /// Erzeugt ein neues Fenster für die Bild- und Tondarstellung.
        /// </summary>
        public BDAWindow()
        {
            // Startup
            InitializeComponent();

            // Finish
            Graph = new DisplayGraph();
        }

        /// <summary>
        /// Legt das Modul der Quelldaten fest und startet die Anzeige.
        /// </summary>
        /// <param name="accessor">Das zu verwendende Modul.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Modul angegeben.</exception>
        /// <exception cref="NotSupportedException">Es ist bereits ein Modul aktiv.</exception>
        public void SetAccessor( AccessModule accessor )
        {
            // Forward
            SetAccessor( accessor, true );
        }

        /// <summary>
        /// Legt das Modul der Quelldaten fest.
        /// </summary>
        /// <param name="accessor">Das zu verwendende Modul.</param>
        /// <param name="autoStart">Gesetzt, wenn die Anzeige sofort beginnen soll.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Modul angegeben.</exception>
        /// <exception cref="NotSupportedException">Es ist bereits ein Modul aktiv.</exception>
        public void SetAccessor( AccessModule accessor, bool autoStart )
        {
            // Validate
            if (null == accessor)
                throw new ArgumentNullException( "accessor" );
            if (null != m_Accessor)
                throw new NotSupportedException();

            // Remember
            m_Accessor = accessor;

            // Create graph
            Graph.CreateGraph();

            // Attach the DirectShow output to us
            Graph.VideoWindow = this;

            // Connect to graph
            m_Accessor.SetGraph( Graph );

            // Initialize the graph rendering engine
            if (autoStart)
                m_Accessor.StartGraph( false, false );
            else
                Graph.Show( false, false );

            // Attach to video window
            var window = (IVideoWindow) Graph.DirectShowObject;

            // Get messages in fullscreen mode
            window.MessageDrain = Handle;
        }

        /// <summary>
        /// Liest oder ändert die Vollbilddarstellung.
        /// </summary>
        [Browsable( false )]
        public bool FullScreen
        {
            get
            {
                // Not valid
                if (DesignMode || (m_Accessor == null))
                    return false;

                // Report
                return Graph.FullScreen;
            }
            set
            {
                // Not valid
                if (DesignMode || (m_Accessor == null))
                    return;

                // No change
                if (value == FullScreen)
                    return;

                // See if we are allowed to do this
                var args = new CancelEventArgs( false );

                // Ask callbacks
                OnFullScreenChanging( args );

                // Aborted
                if (args.Cancel)
                    return;

                // Change
                Graph.FullScreen = value;

                // Inform others
                OnFullScreenChanged( EventArgs.Empty );
            }
        }

        /// <summary>
        /// Wird ausgelöst, bevor sich der Vollbildmodus ändert.
        /// </summary>
        /// <param name="args">Informationen zur Veränderung.</param>
        protected virtual void OnFullScreenChanging( CancelEventArgs args )
        {
            // Fire event
            var sink = FullScreenChanging;
            if (sink != null)
                sink( this, args );
        }

        /// <summary>
        /// Wird nach der Veränderung des Vollbildmodus ausgelöst.
        /// </summary>
        /// <param name="args">Informationen zur Veränderung.</param>
        protected virtual void OnFullScreenChanged( EventArgs args )
        {
            // Fire event
            var sink = FullScreenChanged;
            if (sink != null)
                sink( this, args );
        }

        /// <summary>
        /// Wird nach der Veränderung der Lautsträke ausgelöst.
        /// </summary>
        /// <param name="args">Informationen zur Veränderung.</param>
        protected virtual void OnVolumeChanged( EventArgs args )
        {
            // Fire event
            var sink = VolumeChanged;
            if (sink != null)
                sink( this, args );
        }


        /// <summary>
        /// Aktiviert eine Überlagerung der Bilddarstellung mit einem Bild.
        /// </summary>
        /// <param name="bitmap">Das gewünschte Überladungsbild.</param>
        /// <param name="left">Linke Seite der Überladung.</param>
        /// <param name="top">Oberer Seite der Überladung.</param>
        /// <param name="right">Rechte Seite der Überladung.</param>
        /// <param name="bottom">Untere Seite der Überladung.</param>
        /// <param name="alpha">Transparenzfaktor.</param>
        public void SetOverlay( Bitmap bitmap, double left, double top, double right, double bottom, double alpha )
        {
            // Forward
            SetOverlay( bitmap, left, top, right, bottom, alpha, null );
        }

        /// <summary>
        /// Aktiviert eine Überlagerung der Bilddarstellung mit einem Bild.
        /// </summary>
        /// <param name="bitmap">Das gewünschte Überladungsbild.</param>
        /// <param name="left">Linke Seite der Überladung.</param>
        /// <param name="top">Oberer Seite der Überladung.</param>
        /// <param name="right">Rechte Seite der Überladung.</param>
        /// <param name="bottom">Untere Seite der Überladung.</param>
        /// <param name="alpha">Transparenzfaktor.</param>
        /// <param name="sourceColor">Optional die Transparenzfarbe.</param>
        public void SetOverlay( Bitmap bitmap, double left, double top, double right, double bottom, double alpha, Color? sourceColor )
        {
            // Process
            Graph.SetOverlayBitmap( bitmap, left, top, right, bottom, alpha, sourceColor );
        }

        /// <summary>
        /// Deaktiviert eine Überladung.
        /// </summary>
        public void ClearOverlay()
        {
            // Process
            Graph.ClearOverlayBitmap();
        }

        /// <summary>
        /// Wird für jede Windows Nachricht aufgerufen.
        /// </summary>
        /// <param name="m">Die zu verarbeitende Nachricht.</param>
        protected override void WndProc( ref Message m )
        {
            // Check operation
            switch (m.Msg)
            {
                case 0x0101: ProcessKey( (Keys) m.WParam ); break;
                case 0x0202: ProcessKey( Keys.LButton ); break;
                case 0x0205: ProcessKey( Keys.RButton ); break;
            }

            // Always forward
            base.WndProc( ref m );
        }

        /// <summary>
        /// Wertet eine Taste aus.
        /// </summary>
        /// <param name="key">Die empfangene Taste.</param>
        public void ProcessKey( Keys key )
        {
            // Forward
            KeyProcessor processor;
            if (KeyProcessors.TryGetValue( key, out processor ))
                if (processor != null)
                    processor( key );
        }

        /// <summary>
        /// Wechselt die Vollbilddarstellung.
        /// </summary>
        /// <param name="key">Wird ignoriert.</param>
        public void ProcessFullscreen( Keys key )
        {
            // Process
            FullScreen = !FullScreen;
        }

        /// <summary>
        /// Erhöht die Lautstärke.
        /// </summary>
        /// <param name="key">Wird ignoriert.</param>
        public void VolumeUp( Keys key )
        {
            // Forward
            ChangeVolume( +1 );
        }

        /// <summary>
        /// Verringert die Lautstärke.
        /// </summary>
        /// <param name="key">Wird ignoriert.</param>
        public void VolumeDown( Keys key )
        {
            // Forward
            ChangeVolume( -1 );
        }

        /// <summary>
        /// Ändert die Lautstärke.
        /// </summary>
        /// <param name="delta">Gewünschte Veränderung.</param>
        private void ChangeVolume( int delta )
        {
            // Current value
            int current = Graph.Volume;

            // Get the new volumn
            int volume = current + delta;

            // Not applyable
            if ((volume < 0) || (volume > 100))
                return;

            // Update
            Graph.Volume = (byte) volume;

            // Report
            OnVolumeChanged( EventArgs.Empty );
        }

        /// <summary>
        /// Liest oder setzt die Lautstärke.
        /// </summary>
        [Browsable( false )]
        public double Volume
        {
            get
            {
                // Report
                return (Graph == null) ? 0 : Graph.Volume / 100.0;
            }
            set
            {
                // Store
                if (Graph != null) Graph.Volume = (byte) (100.0 * value);
            }
        }

        /// <summary>
        /// Meldet die Anzahl der in den Graphen eingespielten Daten.
        /// </summary>
        [Browsable( false )]
        public long BytesReceived
        {
            get
            {
                // Read out
                if (DesignMode)
                    return 0;
                else
                    return Graph.InjectorFilter.AVBytesReceived;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der in den Graphen eingespielten Bilddaten.
        /// </summary>
        [Browsable( false )]
        public long VideoBytesReceived
        {
            get
            {
                // Read out
                if (DesignMode)
                    return 0;
                else
                    return Graph.InjectorFilter.VideoBytesReceived;
            }
        }

        /// <summary>
        /// Meldet die Anzahl der in den Graphen eingespielten Tondaten.
        /// </summary>
        [Browsable( false )]
        public long AudioBytesReceived
        {
            get
            {
                // Read out
                if (DesignMode)
                    return 0;
                else
                    return Graph.InjectorFilter.AudioBytesReceived;
            }
        }

        /// <summary>
        /// Liest oder legt fest, ob das spezielle H.264 Bildformat für PowerDVD
        /// verwendet werden soll.
        /// </summary>
        [Browsable( false )]
        public bool? UseCyberlink
        {
            get { return Graph.UseCyberlink; }
            set { Graph.UseCyberlink = value; }
        }


        /// <summary>
        /// Liest oder setzt den Namen des AC3 Decoders.
        /// </summary>
        [Browsable( false )]
        public string AC3Decoder
        {
            get { return Graph.AC3Decoder; }
            set { Graph.AC3Decoder = value; }
        }

        /// <summary>
        /// Liest oder setzt den Namen des MP2 Decoders.
        /// </summary>
        [Browsable( false )]
        public string MP2Decoder
        {
            get { return Graph.MP2Decoder; }
            set { Graph.MP2Decoder = value; }
        }

        /// <summary>
        /// Liest oder setzt den Namen des MPEG-2 Decoders.
        /// </summary>
        [Browsable( false )]
        public string MPEG2Decoder
        {
            get { return Graph.MPEG2Decoder; }
            set { Graph.MPEG2Decoder = value; }
        }

        /// <summary>
        /// Liest oder setzt den Namen des H.264 Decoders.
        /// </summary>
        [Browsable( false )]
        public string H264Decoder
        {
            get { return Graph.H264Decoder; }
            set { Graph.H264Decoder = value; }
        }

        /// <summary>
        /// Liest oder setzt den Versatz zwischen Bild und Ton beim Einspielen
        /// in den Graphen.
        /// </summary>
        [Browsable( false ), DefaultValue( DisplayGraph.DefaultAVDelay )]
        public int AudioVideoDelay
        {
            get { return Graph.AVDelay; }
            set { Graph.AVDelay = value; }
        }

        /// <summary>
        /// Meldet oder legt fest, ob der Graph über zeitliche Bezugspunkte im
        /// Bilddatenstrom informiert werden soll.
        /// </summary>
        [Browsable( false )]
        public bool EnforceVideoSynchronisation
        {
            get { return Graph.EnforceVideoSynchronisation; }
            set { Graph.EnforceVideoSynchronisation = value; }
        }

        /// <summary>
        /// Liest oder setzt die Bildparameter.
        /// </summary>
        [Browsable( false ), DefaultValue( null )]
        public PictureParameters PictureParameters
        {
            get
            {
                // Report
                return (null == Graph) ? null : Graph.PictureParameters;
            }
            set
            {
                // Forward
                if (null != Graph) Graph.PictureParameters = value;
            }
        }

        /// <summary>
        /// Meldet den letzten Zeitstempel, zu dem Daten in den Graphen eingespielt wurden.
        /// </summary>
        [Browsable( false )]
        public TimeSpan? LastPTS
        {
            get
            {
                // Report
                if (Graph == null)
                    return null;
                else
                    return Graph.InjectorFilter.LastPTS;
            }
        }

        /// <summary>
        /// Meldet das aktuelle Seitenverhältnis des Bildes.
        /// </summary>
        [Browsable( false )]
        public double? CurrentAspectRatio
        {
            get
            {
                // Report
                var graph = Graph;
                if (graph == null)
                    return null;
                else
                    return graph.CurrentAspectRatio;
            }
        }

        /// <summary>
        /// Hält den Graphen an.
        /// </summary>
        public void StopGraph()
        {
            // Forward
            Graph.Stop();
        }

        /// <summary>
        /// Meldet den <i>Direct Show</i> Graphen, der zur Anzeige verwendet
        /// wird.
        /// </summary>
        [Browsable( false )]
        public DisplayGraph DisplayGraph
        {
            get
            {
                // Report
                return Graph;
            }
        }
    }
}
