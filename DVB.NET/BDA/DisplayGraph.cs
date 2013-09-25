using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using JMS.DVB.DeviceAccess;
using JMS.DVB.DeviceAccess.BDAElements;
using JMS.DVB.DeviceAccess.Enumerators;
using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.DirectShow.Filters;
using JMS.DVB.DirectShow.Interfaces;


namespace JMS.DVB.DirectShow
{
    /// <summary>
    /// Diese Klasse repräsentiert einen <i>Direct Show</i> Graphen zur Anzeige von
    /// Bild- und Tondaten.
    /// </summary>
    public class DisplayGraph : IDisposable
    {
        /// <summary>
        /// Schalter zur Protokollierung von elementaren Operationen.
        /// </summary>
        public static readonly BooleanSwitch DirectShowTraceSwitch = new BooleanSwitch( "DirectShowTrace", "Reports DirectShow Graph Operations" );

        /// <summary>
        /// Beschreibt das Videobild.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct MPEG2VideoInfo
        {
            /// <summary>
            /// Linke Seite des Quellbereiches.
            /// </summary>
            public Int32 SourceRectLeft;

            /// <summary>
            /// Obere Seite des Quellbereiches.
            /// </summary>
            public Int32 SourceRectTop;

            /// <summary>
            /// Rechte Seite des Quellbereiches.
            /// </summary>
            public Int32 SourceRectRight;

            /// <summary>
            /// Untere Seite des Quellbereiches.
            /// </summary>
            public Int32 SourceRectBottom;

            /// <summary>
            /// Linke Seite des Zielbereiches.
            /// </summary>
            public Int32 TargetRectLeft;

            /// <summary>
            /// Obere Seite des Zielbereiches.
            /// </summary>
            public Int32 TargetRectTop;

            /// <summary>
            /// Rechte Seite des Zielbereiches.
            /// </summary>
            public Int32 TargetRectRight;

            /// <summary>
            /// Untere Seite des Zielbereiches.
            /// </summary>
            public Int32 TargetRectBottom;

            /// <summary>
            /// Datenrate in Bits.
            /// </summary>
            public Int32 BitRate;

            /// <summary>
            /// Rate der Fehler.
            /// </summary>
            public Int32 BitErrorRate;

            /// <summary>
            /// Darstellungsdauer eines Bildes.
            /// </summary>
            public Int64 TimePerFrame;

            /// <summary>
            /// Informationen zur Bildstruktur.
            /// </summary>
            public Int32 InterlaceFlags;

            /// <summary>
            /// Copyrighteinstellungen des Datenstroms.
            /// </summary>
            public Int32 CopyProtectionFlags;

            /// <summary>
            /// Erstes Element des Bildverhältnisses.
            /// </summary>
            public Int32 AspectRatioX;

            /// <summary>
            /// Zweites Element des Bildverhältnisses.
            /// </summary>
            public Int32 AspectRatioY;

            /// <summary>
            /// Steuerinformationen.
            /// </summary>
            public Int32 ControlFlags;

            /// <summary>
            /// Reserviert.
            /// </summary>
            private Int32 _Reserved;

            /// <summary>
            /// Größe des Bildes.
            /// </summary>
            public Int32 BitmpapSize;

            /// <summary>
            /// Breite des Bildes.
            /// </summary>
            public Int32 BitmapWidth;

            /// <summary>
            /// Höhe des Bildes.
            /// </summary>
            public Int32 BitmapHeight;

            /// <summary>
            /// Farbtiefe des Bildes.
            /// </summary>
            public Int16 BitmapPlanes;

            /// <summary>
            /// Anzahl der Bits.
            /// </summary>
            public Int16 BitmapBitCount;

            /// <summary>
            /// Kompressionsart.
            /// </summary>
            public Int32 BitmapCompression;

            /// <summary>
            /// Größe des Bildes.
            /// </summary>
            public Int32 BitmapSizeImage;

            /// <summary>
            /// Erstes Element des Auflösung.
            /// </summary>
            public Int32 BitmapXPelsPerMeter;

            /// <summary>
            /// Zweites Element des Auflösung.
            /// </summary>
            public Int32 BitmapYPelsPerMeter;

            /// <summary>
            /// (unbekannt)
            /// </summary>
            public Int32 BitmapClrUsed;

            /// <summary>
            /// (unbekannt)
            /// </summary>
            public Int32 BitmapClrImportant;

            /// <summary>
            /// Startzeit.
            /// </summary>
            public Int32 StartTimeCode;

            /// <summary>
            /// Anzahl der Bytes im MPEG2 Sequenzkopf.
            /// </summary>
            public Int32 SequenceHeaderBytes;

            /// <summary>
            /// MPEG2 Profil.
            /// </summary>
            public Int32 Profile;

            /// <summary>
            /// MPEG2 Level.
            /// </summary>
            public Int32 Level;

            /// <summary>
            /// Zusätzliche Einstellungen.
            /// </summary>
            public Int32 Flags;

            /// <summary>
            /// Füllfeld.
            /// </summary>
            public Int32 _Padding;

            /// <summary>
            /// Freifeld für den Sequenzkopf.
            /// </summary>
            [MarshalAs( UnmanagedType.ByValArray, SizeConst = 100 )]
            public byte[] _Empty;
        }

        /// <summary>
        /// Beschreibt ein Tondatenformat.
        /// </summary>
        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct AudioFormat
        {
            /// <summary>
            /// Format.
            /// </summary>
            public UInt16 Format;

            /// <summary>
            /// Anzahl der Kanäle.
            /// </summary>
            public UInt16 Channels;

            /// <summary>
            /// Anzahl der Messwerte pro Sekunde.
            /// </summary>
            public UInt32 SamplesPerSec;

            /// <summary>
            /// Anzahl der Bytes pro Sekunde.
            /// </summary>
            public UInt32 BytesPerSec;

            /// <summary>
            /// Speichergranularität.
            /// </summary>
            public UInt16 BlockAlign;

            /// <summary>
            /// Bits pro Messwert.
            /// </summary>
            public UInt16 BitsPerSample;

            /// <summary>
            /// Größe.
            /// </summary>
            public UInt16 Size;

            /// <summary>
            /// Layer.
            /// </summary>
            public UInt16 HeadLayer;

            /// <summary>
            /// Bitrate.
            /// </summary>
            public UInt32 HeadBitrate;

            /// <summary>
            /// Standardmodus.
            /// </summary>
            public UInt16 HeadMode;

            /// <summary>
            /// Erweiterter Modus.
            /// </summary>
            public UInt16 HeadModeExt;

            /// <summary>
            /// Verstärkung.
            /// </summary>
            public UInt16 HeadEmphasis;

            /// <summary>
            /// Detailinformationen.
            /// </summary>
            public UInt16 HeadFlags;

            /// <summary>
            /// Zeitstempel.
            /// </summary>
            public UInt64 PTS;
        }

        /// <summary>
        /// Erzeugt einen <i>Device Context</i>.
        /// </summary>
        /// <param name="hdc">Ein vorhandener Kontext.</param>
        /// <returns>Der angeforderte kompatible Kontext.</returns>
        [DllImport( "gdi32.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr CreateCompatibleDC( IntPtr hdc );

        /// <summary>
        /// Gibt die mit einem <i>Device Context</i> verbundenen Ressourcen wieder frei.
        /// </summary>
        /// <param name="hdc">Ein vorhandener Kontext, der nicht mehr benötigt wird.</param>
        /// <returns>Gesetzt, wenn die Freigabe erfolgreich war.</returns>
        [DllImport( "gdi32.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool DeleteDC( IntPtr hdc );

        /// <summary>
        /// Erzeugt ein Bitfeld für einen <i>Device Context</i>.
        /// </summary>
        /// <param name="hdc">Der zu verwendende Kontext.</param>
        /// <param name="width">Die Breite des Feldes in Pixel.</param>
        /// <param name="height">Die Höhe des Feldes in Pixel.</param>
        /// <returns>Das gewünschte, zum Kontext passende Feld.</returns>
        [DllImport( "gdi32.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr CreateCompatibleBitmap( IntPtr hdc, Int32 width, Int32 height );

        /// <summary>
        /// Aktiviert ein graphisches Objekt.
        /// </summary>
        /// <param name="hdc">Der betroffene <i>Devoce Context</i>.</param>
        /// <param name="bmp">Das zu aktivierende Objekt.</param>
        /// <returns>Das bisher aktive Objekt.</returns>
        [DllImport( "gdi32.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr SelectObject( IntPtr hdc, IntPtr bmp );

        /// <summary>
        /// Gibt die mit einem graphischen Objekt verbunden Ressourcen frei.
        /// </summary>
        /// <param name="obj">Das nicht mehr benötigte Objekt.</param>
        /// <returns>Gesetzt, wenn die Freigabe erfolgreich war.</returns>
        [DllImport( "gdi32.dll" )]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool DeleteObject( IntPtr obj );

        /// <summary>
        /// Der Name des Darstellungsfilters.
        /// </summary>
        private const string Filter_VMR = "VMR";

        /// <summary>
        /// Der Name des Quellfiters.
        /// </summary>
        private const string Filter_Injector = "TS";

        /// <summary>
        /// Verwaltet die globale Anmeldung des Graphen.
        /// </summary>
        private ROTRegister m_Register = null;

        /// <summary>
        /// Verwaltet den eigentlichen Graphen.
        /// </summary>
        private IGraphBuilder m_Graph = null;

        /// <summary>
        /// Alle Filter dieses Graphen.
        /// </summary>
        private Dictionary<string, TypedComIdentity<IBaseFilter>> m_Filters = new Dictionary<string, TypedComIdentity<IBaseFilter>>();

        /// <summary>
        /// Die Protokolldatei für das Erzeugen des Graphen.
        /// </summary>
        private FileStream m_LogFile = null;

        /// <summary>
        /// Die bevorzugte Verzögerung, mit der empfangene Daten abgespielt werden.
        /// </summary>
        public const int DefaultAVDelay = 500;

        /// <summary>
        /// Die Verzögerung, mit der empfangene Daten abgespielt werden.
        /// </summary>
        public int AVDelay = DefaultAVDelay;

        /// <summary>
        /// Der Moniker des bevorzugten H.264 HDTV Videocodecs.
        /// </summary>
        public string H264Decoder = null;

        /// <summary>
        /// Der Moniker des bevorzugten MPEG-2 SDTV Codecs.
        /// </summary>
        public string MPEG2Decoder = null;

        /// <summary>
        /// Der Moniker des bevorzugten MP2 Codecs.
        /// </summary>
        public string MP2Decoder = null;

        /// <summary>
        /// Der Moniker des bevorzugten Dolby Digital (AC3) Codecs.
        /// </summary>
        public string AC3Decoder = null;

        /// <summary>
        /// Beschreibt die nicht lineare Abbildung einer Lautstärkeangabe in Prozent in die
        /// BDA Notation.
        /// </summary>
        private static int[] m_VolumeMap;

        /// <summary>
        /// DVB.NET Filter zur Einspeisung der Daten in den Graphen.
        /// </summary>
        public TSFilter InjectorFilter { get; private set; }

        /// <summary>
        /// Meldet oder legt fest, ob für den Bilddatenstrom Synchronisationspunkte
        /// generiert werden sollen.
        /// </summary>
        public bool EnforceVideoSynchronisation { get; set; }

        /// <summary>
        /// Das zugehörige Fenster.
        /// </summary>
        private Control m_VideoWindow = null;

        /// <summary>
        /// Gesetzt, sobald das Fenster erzeugt wurde.
        /// </summary>
        private bool m_VideoWindowCreated = false;

        /// <summary>
        /// Die aktuelle Datenstromkennung des Bildsignals.
        /// </summary>
        private ushort m_VideoPID = 0;

        /// <summary>
        /// Die aktuelle Datenstromkennung des Tonsignals.
        /// </summary>
        private ushort m_AudioPID = 0;

        /// <summary>
        /// Die Systemuhr.
        /// </summary>
        private volatile NoMarshalComObjects.ReferenceClock m_Clock = null;

        /// <summary>
        /// Die aktuelle Lautstärke.
        /// </summary>
        private byte? m_LastVolume = null;

        /// <summary>
        /// Meldet oder setzt ob das spezielle DirectShow Format für den CyberLink H.264
        /// Video Codec verwendet werden soll.
        /// </summary>
        public bool? UseCyberlink = null;

        /// <summary>
        /// Merke einmal eingestellte Bildparameter und wendet diese beim Neuerzeugen
        /// eines Graphen mit Videobild auf diesen an.
        /// </summary>
        private PictureParameters m_PictureParameters = null;

        /// <summary>
        /// Initialisiert globale Variablen für die Anzeige eines DirectShow Graphen.
        /// </summary>
        static DisplayGraph()
        {
            // Create the map
            m_VolumeMap = new int[101];

            // Fill it
            for (int volume = m_VolumeMap.Length; volume-- > 1; )
                m_VolumeMap[volume] = ((int) Math.Round( 5000 * Math.Log( volume, 10 ) )) - 10000;

            // Silence
            m_VolumeMap[0] = -10000;
        }

        /// <summary>
        /// Erzeugt einen neuen Graphen.
        /// </summary>
        public DisplayGraph()
        {
        }

        /// <summary>
        /// Initialisiert die Filterstruktur.
        /// </summary>
        public void CreateGraph()
        {
            // Cleanup
            DestroyGraph();

            // Check log 
            var logFile = BDASettings.BDALogPath;
            if (logFile != null)
                m_LogFile = new FileStream( logFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read );

            // Create new graph builder
            m_Graph = (IGraphBuilder) Activator.CreateInstance( Type.GetTypeFromCLSID( BDAEnvironment.GraphBuilderClassIdentifier ) );

            // Enable logging
            if (m_LogFile != null)
                m_Graph.SetLogFile( m_LogFile.SafeFileHandle );

            // See if we should register the graph
            m_Register = BDASettings.RegisterBDAGRaph( DirectShowObject, false );

            // Create filter
            InjectorFilter = new TSFilter( this );
            try
            {
                // Check for statistics
                InjectorFilter.EnableStatistics = BDASettings.GenerateTSStatistics;

                // Register in graph
                AddFilter( Filter_Injector, InjectorFilter );
            }
            catch
            {
                // Cleanup
                InjectorFilter.Dispose();

                // Forward
                throw;
            }
        }

        /// <summary>
        /// Bereitet die Anzeige vor.
        /// </summary>
        /// <returns>Gesetzt, wenn die Anzeige bisher noch nicht vorbereitet war.</returns>
        private bool PrepareShow()
        {
            // Once only
            if (m_VideoWindowCreated)
                return false;

            // Lock out
            m_VideoWindowCreated = true;

            // Must have a VMR when displaying video
            CreateVMR();

            // Preload filters
            for (int i = 0; ++i > 0; )
            {
                // Get the name and the filter
                var filterName = string.Format( "Filter{0}", i );
                var filter = ConfigurationManager.AppSettings[filterName];

                // Process
                if (filter == null)
                    break;
                else if (filter.Length > 0)
                    AddFilter( "Preloaded" + filterName, filter );
            }

            // This was the first call
            return true;
        }

        /// <summary>
        /// Lädt einen bestimmten Decoderfilter, sofern explicit konfiguriert.
        /// </summary>
        /// <param name="moniker">Der eindeutige Name des Filters.</param>
        /// <param name="forType">Die Art des Filters - nur zur Benennung im Graphen verwendet.</param>
        /// <param name="processor">Optionale Verarbeitungsmethode für den neuen Filter.</param>
        private void LoadDecoder( string moniker, string forType, Action<TypedComIdentity<IBaseFilter>> processor = null )
        {
            // Not set
            if (string.IsNullOrEmpty( moniker ))
                return;

            // Check it
            var name = string.Format( "{0}Decoder", forType );

            // Process
            using (var decoder = ComIdentity.Create<IBaseFilter>( moniker ))
            {
                // Create
                DirectShowObject.AddFilter( decoder.Interface, name );

                // Call helper
                if (processor != null)
                    processor( decoder );
            }
        }

        /// <summary>
        /// Versucht den DVB.NET Datenstrom direkt mit dem zugehörigen Decoder zu verbinden.
        /// </summary>
        /// <param name="decoder">Ein manuell angelegter Decoder.</param>
        /// <param name="source">Der zu verwendende Ausgang.</param>
        /// <param name="mediaType">Das verwendete Format.</param>
        /// <returns>Gesetzt, wenn die Verbindung aufgebaut wurde.</returns>
        private bool TryDirectConnect( TypedComIdentity<IBaseFilter> decoder, OutputPin source, MediaType mediaType )
        {
            // In normal cases we should directly connect to the filter so try
            var connected = false;

            // Try manual connect
            decoder.InspectAllPins( p => p.QueryDirection() == PinDirection.Input,
                pin =>
                {
                    // Skip on error
                    try
                    {
                        // Get the raw interface for the media type
                        var type = mediaType.GetReference();

                        // Process
                        using (var iFace = ComIdentity.Create<IPin>( pin ))
                            source.Connect( iFace.Interface, type );

                        // Did it
                        connected = true;
                    }
                    catch (Exception)
                    {
                    }

                    // First pin only - even if it can not be used!
                    return false;
                } );

            // Failed
            if (!connected)
                return false;

            // Find the output of the decoder and render it
            decoder.InspectAllPins( p => p.QueryDirection() == PinDirection.Output,
                pin =>
                {
                    // Create helper
                    using (var pinWrapper = ComIdentity.Create<IPin>( pin ))
                        DirectShowObject.Render( pinWrapper.Interface );

                    // Did it
                    return false;
                } );

            // Report
            return connected;
        }

        /// <summary>
        /// Erzuegt den Anzeigegraphen.
        /// </summary>
        /// <param name="mpeg4">Gesetzt für H.264 Bildinformationen - ansonsten wird MPEG-2 erwartet.</param>
        /// <param name="ac3">Gesetzt für AC3 Toninformationen - ansonsten wird MP2 erwartet.</param>
        public void Show( bool mpeg4, bool ac3 )
        {
            // Startup
            bool firstCall = PrepareShow();

            // Leave fullscreen before stopping graph
            bool fullscreen = FullScreen;
            if (fullscreen)
                FullScreen = false;

            // Stop the graph
            Stop();

            // Forget the clock
            DisposeClock();

            // Remove all filters not created by us
            var noRemove = new HashSet<IntPtr>( m_Filters.Values.Select( f => f.Interface ) );
            ((IFilterGraph) m_Graph).InspectFilters( filter =>
                {
                    // Create report structure
                    var info = new FilterInfo();

                    // Load it
                    filter.QueryFilterInfo( ref info );

                    // Forget about graph
                    BDAEnvironment.Release( ref info.Graph );

                    // Process using raw interfaces
                    using (var id = ComIdentity.Create( filter ))
                        if (!noRemove.Contains( id.Interface ))
                            try
                            {
                                // Repoert
                                if (DirectShowTraceSwitch.Enabled)
                                    Trace.WriteLine( string.Format( Properties.Resources.Trace_RemoveFilter, info.Name ), DirectShowTraceSwitch.DisplayName );

                                // Process
                                DirectShowObject.RemoveFilter( id.Interface );
                            }
                            catch (Exception e)
                            {
                                // Report
                                Trace.WriteLine( string.Format( Properties.Resources.Trace_Exception_RemoveFilter, info.Name, e.Message ), DirectShowTraceSwitch.DisplayName );
                            }
                } );

            // Create media types
            var videoType = CreateVideoType( mpeg4, UseCyberlink );
            var audioType = CreateAudioType( ac3 );

            // Configure injection pins
            InjectorFilter.SetAudioType( audioType );
            InjectorFilter.SetVideoType( videoType );

            // Helper
            var audioConnected = false;
            var videoConnected = false;

            // Pre-loaded decoders
            LoadDecoder( mpeg4 ? H264Decoder : MPEG2Decoder, mpeg4 ? "HDTV-Video" : "SDTV-Video", decoder => videoConnected = TryDirectConnect( decoder, InjectorFilter.VideoPin, videoType ) );
            LoadDecoder( ac3 ? AC3Decoder : MP2Decoder, ac3 ? "AC3-Audio" : "MP2-Audio", decoder => audioConnected = TryDirectConnect( decoder, InjectorFilter.AudioPin, audioType ) );

            // Create display
            if (!audioConnected)
                DirectShowObject.Render( InjectorFilter.AudioPin.Interface );
            if (!videoConnected)
                DirectShowObject.Render( InjectorFilter.VideoPin.Interface );

            // Show for the first time
            var vmr = VMR;
            if (firstCall)
                if (vmr != null)
                    if (m_VideoWindow != null)
                    {
                        // Respect window settings
                        vmr.ClippingWindow = m_VideoWindow;
                        vmr.AdjustSize( m_VideoWindow );
                    }

            // Use it
            m_Clock = new NoMarshalComObjects.ReferenceClock( Activator.CreateInstance( Type.GetTypeFromCLSID( Constants.CLSID_SystemClock ) ), true );

            // Attach to graph
            GraphAsFilter.SetSyncSource( m_Clock.ComInterface );

            // Time to restart the graph
            Run();

            // Reinstall volume
            if (m_LastVolume.HasValue)
                Volume = m_LastVolume.Value;

            // Reinstall picture parameters
            if (m_PictureParameters != null)
                if (vmr != null)
                    m_PictureParameters.Update( vmr );

            // Back to full screen mode
            if (fullscreen)
                FullScreen = true;
        }

        /// <summary>
        /// Liest oder verändert die Darstellungsparameter des Videobildes.
        /// </summary>
        /// <exception cref="ArgumentNullException">Die Eigenschaft darf nicht <i>null</i> sein.</exception>
        public PictureParameters PictureParameters
        {
            get
            {
                // None
                var vmr = VMR;
                if (vmr == null)
                    return null;

                // Be safe
                try
                {
                    // Always create new
                    return new PictureParameters( vmr );
                }
                catch
                {
                    // Maybe there is no hardware support
                    return null;
                }
            }
            set
            {
                // Validate
                if (value == null)
                    throw new ArgumentNullException( "value" );

                // Remember last setting
                m_PictureParameters = value;

                // Store
                var vmr = VMR;
                if (vmr != null)
                    value.Update( vmr );
            }
        }

        /// <summary>
        /// Liest oder verändert die Lautstärke.
        /// </summary>
        public byte Volume
        {
            get
            {
                // Find audio control
                var audio = (IBasicAudio) DirectShowObject;
                if (audio == null)
                    return 0;

                // Load the data
                int current = audio.Volume;

                // Find the first match
                for (int i = m_VolumeMap.Length; i-- > 0; )
                    if (current >= m_VolumeMap[i])
                        return (byte) i;

                // Must be silent
                return 0;
            }
            set
            {
                // Correct
                if (value > 100)
                    value = 100;

                // Remember
                m_LastVolume = value;

                // Be safe
                try
                {
                    // Find audio control
                    var audio = (IBasicAudio) DirectShowObject;
                    if (audio != null)
                        audio.Volume = m_VolumeMap[value];
                }
                catch
                {
                    // Ignore any error
                }
            }
        }

        /// <summary>
        /// Legt fest, wo die Ausgabe des Bilds erfolgen soll.
        /// </summary>
        public Control VideoWindow
        {
            set
            {
                // Remember
                m_VideoWindow = value;

                // Register
                m_VideoWindow.SizeChanged += ( s, e ) => { var v = VMR; if (v != null) v.AdjustSize( m_VideoWindow ); };

                // Connect
                var vmr = VMR;
                if (vmr != null)
                    vmr.ClippingWindow = m_VideoWindow;
            }
        }

        /// <summary>
        /// Erzeugt die Anzeigekomponent des Graphen.
        /// </summary>
        private void CreateVMR()
        {
            // Test state
            var vmr = VMR;
            if (vmr != null)
                return;

            // Create the VMR
            vmr = VMR.Create();
            try
            {
                // Add the VMR
                AddFilter( Filter_VMR, vmr );
            }
            catch
            {
                // Cleanup
                vmr.Dispose();

                // Forward
                throw;
            }
        }

        /// <summary>
        /// Meldet das aktuelle Seitenverhältnis des Bildes.
        /// </summary>
        public double? CurrentAspectRatio
        {
            get
            {
                // Report
                var vmr = VMR;
                if (vmr == null)
                    return null;
                else
                    return vmr.CurrentAspectRatio;
            }
        }

        /// <summary>
        /// Gibt alle internen Strukturen frei.
        /// </summary>
        private void DestroyGraph()
        {
            // Get rid of clock
            DisposeClock();

            // Try stop and delete all filters
            if (null != m_Graph)
                try
                {
                    // Stop self
                    GraphAsFilter.Stop();
                }
                catch
                {
                }

            // Filters first
            foreach (var filter in m_Filters.Values)
                filter.Dispose();

            // Clear list
            m_Filters.Clear();

            // Forward
            using (m_Register)
                m_Register = null;

            // Must cleanup COM references
            BDAEnvironment.Release( ref m_Graph );

            // Must cleanup file
            using (m_LogFile)
                m_LogFile = null;

            // Clear helpers and reset flags
            m_VideoWindowCreated = false;
            m_VideoPID = 0;
            m_AudioPID = 0;

            // TS Filter
            using (InjectorFilter)
                InjectorFilter = null;
        }

        /// <summary>
        /// Legt die Datenstromkennungen für Bild und Ton fest.
        /// </summary>
        /// <param name="video">Die Datenstromkennung für das Bild.</param>
        /// <param name="audio">Die Datenstromkennung für den Ton.</param>
        public void SetPIDs( ushort video, ushort audio )
        {
            // Remember
            m_VideoPID = video;
            m_AudioPID = audio;
        }

        /// <summary>
        /// Liest oder setzt die Vollbilddarstellung.
        /// </summary>
        public bool FullScreen
        {
            get
            {
                // Get the video window
                var video = DirectShowObject as IVideoWindow;

                // Report
                return (video.FullScreenMode != 0);
            }
            set
            {
                // Get the video window
                var video = DirectShowObject as IVideoWindow;

                // Change fullscreen mode
                video.FullScreenMode = value ? -1 : 0;
            }
        }

        /// <summary>
        /// Deaktiviert die Bildüberlagerung.
        /// </summary>
        public void ClearOverlayBitmap()
        {
            // Create structure
            var param = new VMRAlphaBitmap { Flags = VMRAlphaBitmapFlags.Disable };

            // Forward
            using (var vmr = VMR.MarshalToManaged())
                ((IVMRMixerBitmap) vmr.Object).SetAlphaBitmap( ref param );
        }

        /// <summary>
        /// Aktiviert die Bildüberlagerung.
        /// </summary>
        /// <param name="bitmap">Die gewünschte Überlagerung.</param>
        /// <param name="left">Die linke Seite.</param>
        /// <param name="top">Die obere Seite.</param>
        /// <param name="right">Die rechte Seite.</param>
        /// <param name="bottom">Die untere Seite.</param>
        /// <param name="alpha">Der Transparenzfaktor.</param>
        public void SetOverlayBitmap( Bitmap bitmap, double left, double top, double right, double bottom, double alpha )
        {
            // Forward
            SetOverlayBitmap( bitmap, left, top, right, bottom, alpha, null );
        }

        /// <summary>
        /// Aktiviert die Bildüberlagerung.
        /// </summary>
        /// <param name="bitmap">Die gewünschte Überlagerung.</param>
        /// <param name="left">Die linke Seite.</param>
        /// <param name="top">Die obere Seite.</param>
        /// <param name="right">Die rechte Seite.</param>
        /// <param name="bottom">Die untere Seite.</param>
        /// <param name="alpha">Der Transparenzfaktor.</param>
        /// <param name="sourceColor">Optional der Transparenzschlüssel.</param>
        public void SetOverlayBitmap( Bitmap bitmap, double left, double top, double right, double bottom, double alpha, Color? sourceColor )
        {
            // Attach to video window device context
            using (var self = Graphics.FromHwnd( m_VideoWindow.Handle ))
            {
                // Get the device context
                IntPtr originalHDC = self.GetHdc();
                try
                {
                    // Create the HDC
                    IntPtr compat = CreateCompatibleDC( originalHDC );
                    try
                    {
                        // Create the memory device context
                        using (var inMem = Graphics.FromHdc( compat ))
                        {
                            // Get the device context
                            IntPtr memoryHDC = inMem.GetHdc();
                            try
                            {
                                // Get the bitmap
                                IntPtr bmp = bitmap.GetHbitmap();
                                try
                                {
                                    // Select the bitmap into it
                                    IntPtr prev = SelectObject( memoryHDC, bmp );
                                    try
                                    {
                                        // Create structure
                                        var param =
                                            new VMRAlphaBitmap
                                            {
                                                Target = { Left = (float) left, Top = (float) top, Bottom = (float) bottom, Right = (float) right, },
                                                Source = { Right = bitmap.Width, Bottom = bitmap.Height },
                                                Flags = VMRAlphaBitmapFlags.DeviceHandle,
                                                DeviceHandle = memoryHDC,
                                                Alpha = (float) alpha,
                                            };

                                        // Set the source color
                                        if (sourceColor.HasValue)
                                        {
                                            // Finish
                                            param.Flags |= VMRAlphaBitmapFlags.SourceColorKey;
                                            param.SourceColorKey = (uint) sourceColor.Value.ToArgb();
                                        }

                                        // Activate
                                        using (var vmr = VMR.MarshalToManaged())
                                            ((IVMRMixerBitmap) vmr.Object).SetAlphaBitmap( ref param );
                                    }
                                    finally
                                    {
                                        // Restore
                                        SelectObject( memoryHDC, prev );
                                    }
                                }
                                finally
                                {
                                    // Cleanup
                                    DeleteObject( bmp );
                                }
                            }
                            finally
                            {
                                // Release resources
                                inMem.ReleaseHdc();
                            }
                        }
                    }
                    finally
                    {
                        // Release HDC
                        DeleteDC( compat );
                    }
                }
                finally
                {
                    // Release resources
                    self.ReleaseHdc();
                }
            }
        }

        /// <summary>
        /// Gibt den Zähler der Zeitbasis frei.
        /// </summary>
        private void DisposeClock()
        {
            // Process
            using (m_Clock)
                m_Clock = null;
        }

        /// <summary>
        /// Beendet die Nutzung dieses Graphen endgültig.
        /// </summary>
        public void Dispose()
        {
            // Do cleanups
            DestroyGraph();
        }

        /// <summary>
        /// Meldet die aktuelle Systemzeit des Graphen oder <i>null</i>, wenn diese nicht ermittelt
        /// werden kann.
        /// </summary>
        public long? SystemClock
        {
            get
            {
                // Report
                var clock = m_Clock;
                if (clock == null)
                    return null;
                else
                    return clock.Time;
            }
        }

        /// <summary>
        /// Erzeugt den Datentyp für ein Bildsignal.
        /// </summary>
        /// <param name="mpeg4">Gesetzt für HDTV.</param>
        /// <param name="useCyberlink">Gesetzt für die Nutzung des Cyberlink Formates.</param>
        /// <returns>Das gewünschte Datenformat.</returns>
        private static MediaType CreateVideoType( bool mpeg4, bool? useCyberlink )
        {
            // Create format
            var fakeFormat = new MPEG2VideoInfo { BitmpapSize = 40 };

            // Resulting media type
            MediaType type;

            // Check mode
            if (mpeg4)
            {
                // Set it up
                fakeFormat.BitmapWidth = 1980;
                fakeFormat.BitmapHeight = 1088;

                // Check flag
                if (!useCyberlink.HasValue)
                {
                    // See if we should use cyberlink format specs
                    string cl = ConfigurationManager.AppSettings["UseCyberLink"];

                    // Copy if set
                    useCyberlink = (!string.IsNullOrEmpty( cl ) && bool.Parse( cl ));
                }

                // Check mode
                if (useCyberlink.Value)
                {
                    // Create media type for video
                    type = new MediaType( Constants.KSDATAFORMAT_TYPE_VIDEO, Constants.KSDATAFORMAT_SUBTYPE_H264_VIDEO_Cyberlink, Constants.FORMAT_MPEG2_VIDEO, true, 0x10000 );
                }
                else
                {
                    // Create media type for video
                    type = new MediaType( Constants.KSDATAFORMAT_TYPE_VIDEO, Constants.KSDATAFORMAT_SUBTYPE_H264_VIDEO, Constants.FORMAT_MPEG2_VIDEO, true, 0x10000 );
                }
            }
            else
            {
                // Set it up
                fakeFormat.BitmapWidth = 720;
                fakeFormat.BitmapHeight = 576;

                // Create media type for video
                type = new MediaType( Constants.KSDATAFORMAT_TYPE_VIDEO, Constants.KSDATAFORMAT_SUBTYPE_MPEG2_VIDEO, Constants.FORMAT_MPEG2_VIDEO, true, 0x10000 );
            }

            // Finish
            type.SetFormat( fakeFormat );

            // Report
            return type;
        }

        /// <summary>
        /// Erzeugt ein Datenformat für den Ton.
        /// </summary>
        /// <param name="ac3">Gesetzt für Dolby Digital.</param>
        /// <returns>Das gewünschte Format.</returns>
        private static MediaType CreateAudioType( bool ac3 )
        {
            // Create format
            var fakeFormat =
                new AudioFormat
                    {
                        SamplesPerSec = 48000,
                        Channels = 2,
                        Size = 22,
                    };

            // Create format information
            var type = new MediaType( Constants.KSDATAFORMAT_TYPE_AUDIO, ac3 ? Constants.KSDATAFORMAT_SUBTYPE_AC3_AUDIO : Constants.KSDATAFORMAT_SUBTYPE_MPEG2_AUDIO, Constants.FORMAT_WaveFormatEx, true, 0x10000 );

            // Finish
            type.SetFormat( fakeFormat );

            // Report
            return type;
        }

        /// <summary>
        /// Meldet einen Filter in Graphen an.
        /// </summary>
        /// <param name="name">Der Name des Filters.</param>
        /// <param name="filter">Die Verwaltungsinstanz des Filters.</param>
        private void AddFilter( string name, TypedComIdentity<IBaseFilter> filter )
        {
            // Test
            if (string.IsNullOrEmpty( name ))
                throw new ArgumentNullException( "name" );
            if (filter == null)
                throw new ArgumentNullException( "filter" );

            // Test
            if (m_Graph == null)
                throw new InvalidOperationException( "not initialized" );

            // Forward to COM
            m_Graph.AddFilter( filter.Interface, name );

            // Remember
            m_Filters.Add( name, filter );
        }

        /// <summary>
        /// Erzeugt einen Filter und vermerkt ihn.
        /// </summary>
        /// <param name="name">Der Name des Filters.</param>
        /// <param name="moniker">Der eindeutige Name des Filters.</param>
        private void AddFilter( string name, string moniker )
        {
            // Test
            if (string.IsNullOrEmpty( name ))
                throw new ArgumentNullException( "name" );
            if (string.IsNullOrEmpty( moniker ))
                throw new ArgumentNullException( "moniker" );

            // Create
            var filter = ComIdentity.Create<IBaseFilter>( moniker );
            try
            {
                // Process
                AddFilter( name, filter );
            }
            catch
            {
                // Cleanup
                filter.Dispose();

                // Forward
                throw;
            }
        }

        /// <summary>
        /// Startet den Graphen.
        /// </summary>
        /// <exception cref="InvalidOperationException">Der Graph konnte nicht gestartet werden.</exception>
        public void Run()
        {
            // Try start
            int result = GraphAsFilter.Run( 0 );

            // Run the graph
            if (result < 0)
                throw new InvalidOperationException( string.Format( "graph not started, error is 0x{0:x}", result ) );
        }

        /// <summary>
        /// Hält den Graphen an - Fehler werden dabei ignoriert.
        /// </summary>
        public virtual void Stop()
        {
            // Forward - no error detectable
            GraphAsFilter.Stop();
        }

        /// <summary>
        /// Unterbricht den Graphen.
        /// </summary>
        public void Pause()
        {
            // Forward
            GraphAsFilter.Pause();
        }

        /// <summary>
        /// Meldet die Filterschnittstelle des Graphen.
        /// </summary>
        public IMediaFilter GraphAsFilter
        {
            get
            {
                // Report
                return (IMediaFilter) DirectShowObject;
            }
        }

        /// <summary>
        /// Meldet die primäre Schnittstelle zum Aufbau des Graphen.
        /// </summary>
        public IGraphBuilder DirectShowObject
        {
            get
            {
                // Report
                return m_Graph;
            }
        }

        /// <summary>
        /// Meldet den aktuellen Darstellungsfilter.
        /// </summary>
        private VMR VMR
        {
            get
            {
                // Load
                TypedComIdentity<IBaseFilter> vmr;
                if (m_Filters.TryGetValue( Filter_VMR, out vmr ))
                    return (VMR) vmr;
                else
                    return null;
            }
        }

        /// <summary>
        /// Ermittelt alle MPEG2 Decoder Filter.
        /// </summary>
        public static IDeviceOrFilterInformation[] VideoDecoderFilters
        {
            get
            {
                // Forward
                return FindFilterByDecoderType( Constants.KSDATAFORMAT_TYPE_VIDEO, Constants.KSDATAFORMAT_SUBTYPE_MPEG2_VIDEO );
            }
        }

        /// <summary>
        /// Ermittelt alle MP2 Decoder Filter.
        /// </summary>
        public static IDeviceOrFilterInformation[] AudioDecoderFilters
        {
            get
            {
                // Forward
                return FindFilterByDecoderType( Constants.KSDATAFORMAT_TYPE_AUDIO, Constants.KSDATAFORMAT_SUBTYPE_MPEG2_AUDIO );
            }
        }

        /// <summary>
        /// Ermittelt alle H.264 Decoder Filter.
        /// </summary>
        public static IDeviceOrFilterInformation[] HDTVDecoderFilters
        {
            get
            {
                // Forward
                return FindFilterByDecoderType
                    (
                        Constants.KSDATAFORMAT_TYPE_VIDEO, Constants.KSDATAFORMAT_SUBTYPE_H264_VIDEO,
                        Constants.KSDATAFORMAT_TYPE_VIDEO, Constants.KSDATAFORMAT_SUBTYPE_AVC1_VIDEO
                    );
            }
        }

        /// <summary>
        /// Ermittelt alle AC3 Decoder Filter.
        /// </summary>
        public static IDeviceOrFilterInformation[] AC3DecoderFilters
        {
            get
            {
                // Forward
                return FindFilterByDecoderType( Constants.KSDATAFORMAT_TYPE_AUDIO, Constants.KSDATAFORMAT_SUBTYPE_AC3_AUDIO );
            }
        }

        /// <summary>
        /// Ermittelt alle Decoder Filter, die ein bestimmtes Format unterstützen.
        /// </summary>
        /// <param name="filters">Filterbedingung für die Suche.</param>
        /// <returns>Alle zur Bedingung passenden Filter.</returns>
        private static IDeviceOrFilterInformation[] FindFilterByDecoderType( params Guid[] filters )
        {
            // Validate
            if (0 != (filters.Length % 2))
                throw new ArgumentException( "filters" );

            // Create result
            var list = new List<FilterInformation>();

            // Create the filter mapper
            var mapper = (IFilterMapper2) Activator.CreateInstance( Type.GetTypeFromCLSID( Constants.CLSID_FilterMapper2 ) );
            try
            {
                // Lock in memory
                var fixFilter = GCHandle.Alloc( filters, GCHandleType.Pinned );
                try
                {
                    // Request enumerator
                    IEnumMoniker monikers = null;
                    mapper.EnumMatchingFilters( out monikers, 0, 1, 0x200001, 1, filters.Length / 2, fixFilter.AddrOfPinnedObject(), IntPtr.Zero, IntPtr.Zero, 0, 1, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero );

                    // Add all to the list using device and filter helper
                    DeviceAndFilterInformations.Cache.LoadFromEnumeration( Guid.Empty, "CLSID", list, monikers );
                }
                finally
                {
                    // Unlock memory
                    fixFilter.Free();
                }
            }
            finally
            {
                // Back to COM
                BDAEnvironment.Release( ref mapper );
            }

            // Report
            return list.ToArray();
        }
    }
}
