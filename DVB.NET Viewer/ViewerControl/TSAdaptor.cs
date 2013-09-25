using System;
using System.Collections.Generic;
using JMS.DVB.DirectShow.AccessModules;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Verbindet die Anwendung mit einem Transport Stream, aus dem die
    /// Tonspuren dynamisch ermittelt werden müssen.
    /// </summary>
    public abstract class TSAdaptor : Adaptor
    {
        /// <summary>
        /// Name der Tonspur, die nach dem Starten des Graphen aktiviert werden soll.
        /// </summary>
        private string m_DelayAudio = null;

        /// <summary>
        /// Abbildung der Namen der Tonspuren auf den Index im Transport Stream.
        /// </summary>
        private Dictionary<string, int> m_AudioMap = new Dictionary<string, int>();

        /// <summary>
        /// Erzeugt eine neue Zugriffsinstanz.
        /// </summary>
        /// <param name="main">Die zugehörige Anwendung.</param>
        public TSAdaptor( IViewerSite main )
            : base( main )
        {
        }

        /// <summary>
        /// Wird immer aufgerufen, wenn innerhalb des Zugriffsmoduls neue Daten angefordert
        /// werden.
        /// </summary>
        /// <param name="endPoint">Aktuelles Zugriffsmodul.</param>
        protected virtual void OnWaitData( TransportStreamReceiver endPoint )
        {
            // No longer needed
            DisconnectWaiter();
        }

        /// <summary>
        /// Die Datenquelle ist fertig configuriert.
        /// </summary>
        protected void OnConnected()
        {
            // Register display
            Accessor.StreamChanged += restartGraph => Parent.Invoke( new TransportStreamAccessor.StreamChangedHandler( StreamChanged ), restartGraph );

            // Register Videotext analysis
            Accessor.OnVideotextData += VideoText.AddPayload;
        }

        /// <summary>
        /// Blendet das OSD aus.
        /// </summary>
        private void HideOSD()
        {
            // Hide message
            var osd = Parent as IOSDSite;
            if (osd != null)
                osd.Hide();
        }

        /// <summary>
        /// Der Graph muß neu aufgebaut werden - allerdings auf dem Hauptthread
        /// der Anwendung.
        /// </summary>
        /// <param name="restartGraph">Gesetzt, wenn der Direct Show Graph neu
        /// aufgebaut werden soll.</param>
        private void StreamChanged( bool restartGraph )
        {
            // With cleanup
            try
            {
                // Load audio map
                LoadAudio();

                // Restart videotext collector
                VideoText.Deactivate( true );

                // Disable all OSD
                HideOSD();

                // Restart graph
                Accessor.SetDecoder();

                // Select audio
                if (m_AudioMap.Count > 0)
                    if (m_DelayAudio != null)
                        ShowMessage( SetAudio( m_DelayAudio ), Properties.Resources.NameTitle, true );
            }
            finally
            {
                // Discard
                m_DelayAudio = null;
            }
        }

        /// <summary>
        /// Alle Tonspuren laden.
        /// </summary>
        protected void LoadAudio()
        {
            // Load the names
            var audioNames = Accessor.GetAudioNames();

            // See if something changed
            var changed = (audioNames.Length != m_AudioMap.Count);

            // Check in detail
            if (!changed)
                foreach (var item in m_AudioMap)
                    if (StringComparer.InvariantCultureIgnoreCase.Equals( item.Key, audioNames[item.Value] ))
                    {
                        // Something changed
                        changed = true;

                        // No need to check any more
                        break;
                    }

            // Must reload
            if (changed)
            {
                // Reset map
                m_AudioMap.Clear();

                // Process all names
                foreach (var audioName in audioNames)
                    m_AudioMap[audioName] = m_AudioMap.Count;
            }

            // Reset lists
            Favorites.ClearAudioAndService();

            // Process all names
            foreach (var audioName in audioNames)
                Favorites.AddAudio( audioName, false );

            // Finished
            Favorites.FillAudioList();
        }

        /// <summary>
        /// Das TCP/IP UDP Modul zur Übertragung des Transport Streams an den DirectShow
        /// Graphen.
        /// </summary>
        public new TransportStreamAccessor Accessor { get { return (TransportStreamAccessor) base.Accessor; } }

        /// <summary>
        /// Meldet, ob ein Videotext Signal vorliegt.
        /// </summary>
        public override bool TTXAvailable { get { return (null != Accessor) && Accessor.TTXAvailable; } }

        /// <summary>
        /// Liest oder setzt den aktuellen Eintrag zur Programmzeitschrift.
        /// </summary>
        public override EPG.EventEntry CurrentEntry
        {
            get
            {
                // Attach to accessor
                var accessor = Accessor;

                // Process
                return (accessor == null) ? base.CurrentEntry : accessor.CurrentEntry;
            }
            set
            {
                // Process
                var accessor = Accessor;
                if (accessor == null)
                    base.CurrentEntry = value;
                else
                    accessor.CurrentEntry = value;
            }
        }

        /// <summary>
        /// Liest oder setzt den Folgeeintrag zur Programmzeitschrift.
        /// </summary>
        public override EPG.EventEntry NextEntry
        {
            get
            {
                // Attach to accessort
                var accessor = Accessor;

                // Process
                return (accessor == null) ? base.NextEntry : accessor.NextEntry;
            }
            set
            {
                // Process
                var accessor = Accessor;
                if (accessor == null)
                    base.NextEntry = value;
                else
                    accessor.NextEntry = value;
            }
        }

        /// <summary>
        /// Speichert die zuletzt verwendete Tonspur.
        /// </summary>
        protected virtual string LastAudio { set { } }

        /// <summary>
        /// Wählte eine andere Tonspur.
        /// </summary>
        /// <param name="audio">Voller Name der Tonspur oder <i>null</i> für die
        /// bevorzugte Tonspur.</param>
        /// <param name="updateSettings">Gesetzt, wenn die neue Tonspur auch
        /// in den Einstellungen für den nächsten Start vermerkt werden soll.</param>
        /// <returns>Sendername mit ausgewählter Tonspur oder <i>null</i>.</returns>
        protected string SetAudio( string audio, bool updateSettings )
        {
            // Index to use
            var audioIndex = 0;

            // Not yet loaded
            if (m_AudioMap.Count < 1)
            {
                // Delay load
                m_DelayAudio = audio ?? string.Empty;

                // Do not show
                audio = null;
            }
            else
            {
                // Test mode
                if (updateSettings) 
                    LastAudio = audio;

                // Try to find default audio
                if (string.IsNullOrEmpty( audio )) 
                    audio = Favorites.GetPreferredAudio();

                // Try to find in map
                if (!string.IsNullOrEmpty( audio ))
                    if (!m_AudioMap.TryGetValue( audio, out audioIndex ))
                    {
                        // Reset
                        audioIndex = 0;

                        // Do not show
                        audio = null;
                    }
            }

            // Activate
            Accessor.AudioIndex = audioIndex;

            // Test
            if (string.IsNullOrEmpty( audio ))
                return StationName;
            else
                return string.Format( "{0} - {1}", StationName, audio );
        }

        /// <summary>
        /// Aktiviert den Direct Show Graphen nach einem Senderwechsel mit der ersten
        /// Tonspur.
        /// </summary>
        /// <param name="preserveAudio">Wird gesetzt, um die Tonspu beizubehalten.</param>
        public string RestartAudio( bool preserveAudio )
        {
            // Audio to restore
            string audio = null;

            // Check for current audio
            if (preserveAudio)
                foreach (var info in m_AudioMap)
                    if (info.Value == Accessor.AudioIndex)
                    {
                        // Use this
                        audio = info.Key;

                        // Done
                        break;
                    }

            // Clear map
            m_AudioMap.Clear();

            // Choose primary track
            Accessor.AudioIndex = 0;

            // Reset anything in queue
            Accessor.ClearBuffers();

            // Connect the waiter
            ConnectWaiter();

            // Start sending data
            Accessor.Start();

            // Prepare to load default audio channel
            return SetAudio( audio, false );
        }

        /// <summary>
        /// Aktiviert die Überwachung eines gesteuerten Eingangsdatenstroms.
        /// </summary>
        public virtual void ConnectWaiter()
        {
        }

        /// <summary>
        /// Deaktiviert die Überwachung eines gesteuerten Eingangsdatenstroms.
        /// </summary>
        public virtual void DisconnectWaiter()
        {
        }
    }
}
