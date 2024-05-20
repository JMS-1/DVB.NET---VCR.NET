using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Verbindet sich mit einer laufenden Aufzeichnung im VCR.NET Recording Service.
    /// </summary>
    public class CurrentConnector : VCRConnector
    {
        /// <summary>
        /// Informationen zu einer Aufzeichnung und der Zuordnung innerhalb einer
        /// Mehrkanalaufzeichnung.
        /// </summary>
        private class JobScheduleInfo
        {
            /// <summary>
            /// Die zugehörige Aktivität.
            /// </summary>
            public readonly VCRNETRestProxy.Current Activity;

            /// <summary>
            /// Erstellt eine neue Beschreibung.
            /// </summary>
            /// <param name="info">Die zugehörige Aktivität.</param>
            public JobScheduleInfo(VCRNETRestProxy.Current info)
            {
                // Remember
                Activity = info;
            }

            /// <summary>
            /// Die Position innerhalb der Mehrkanalaufzeichnung.
            /// </summary>
            public int StreamIndex { get { return Activity.streamIndex; } }

            /// <summary>
            /// Meldet den Namen der Aktivität.
            /// </summary>
            public string Name { get { return Activity.name; } }

            /// <summary>
            /// Meldet den vollen Pfad zur ersten Datei.
            /// </summary>
            public string PrimaryPath { get { return (Activity.files ?? Enumerable.Empty<string>()).FirstOrDefault(); } }

            /// <summary>
            /// Erzeugt einen Anzeigenamen.
            /// </summary>
            /// <returns>Ein Anzeigename aus den Namen von Aufzeichnung (optional)
            /// und Auftrag.</returns>
            public override string ToString()
            {
                // Report
                return Name;
            }

            /// <summary>
            /// Aktiviert den Netzwerkempfang.
            /// </summary>
            /// <param name="endPoint">Der zugehörige <i>VCR.NET Recording Service</i>.</param>
            /// <param name="target">Die Zieladresse des Netzwerkversands.</param>
            public void StreamTo(string endPoint, string target)
            {
                // Forward
                VCRNETRestProxy.SetStreamTargetSync(endPoint, Activity.device, Activity.source, Activity.referenceId.Value, target);
            }
        }

        /// <summary>
        /// Aktuell angezeigter Teil der Mehrkanalaufzeichnung.
        /// </summary>
        private JobScheduleInfo m_CurrentSource = null;

        /// <summary>
        /// Erste Teilaufzeichnung der laufenden Aufzeichnung.
        /// </summary>
        private string m_DefaultStation = null;

        /// <summary>
        /// Vorgabe für die Auswahl der Teilaufzeichnung.
        /// </summary>
        private string m_StartupStation;

        /// <summary>
        /// Aktuelle Aufzeichnung.
        /// </summary>
        private VCRNETRestProxy.Current[] m_allActivities = null;

        /// <summary>
        /// Gesetzt, sobald die aktuelle Aufzeichnung bekannt ist.
        /// </summary>
        private bool m_allActivitiesValid = false;

        /// <summary>
        /// Erzeugt eine Kommunikationseinheit.
        /// </summary>
        /// <param name="adaptor">Verbindung zur aktuellen Anwendung.</param>
        /// <param name="startWith">Vorgabe für die Auswahl der ersten Teilaufzeichnung.</param>
        public CurrentConnector(VCRAdaptor adaptor, string startWith)
            : base(adaptor)
        {
            // Remember
            m_StartupStation = startWith;
        }

        /// <summary>
        /// Aktualisiert die Daten der aktuellen Aufzeichnung.
        /// </summary>
        /// <param name="current">Die aktuellen Daten.</param>
        private void ReceiveCurrentRecording(VCRNETRestProxy.Current[] current)
        {
            // Remember if valid
            m_allActivities = current;
            m_allActivitiesValid = true;
        }

        /// <summary>
        /// Aktiviert zusätzliche Tastaturbefehle.
        /// </summary>
        public override void FillOptions()
        {
            // Forward
            base.FillOptions();

            // Register
            Adaptor.Parent.SetKeyHandler(Keys.Multiply, StartTimeshift);
        }

        /// <summary>
        /// Schaltet um in das zeitversetzte Betrachten einer Teilaufzeichnung.
        /// </summary>
        private void StartTimeshift()
        {
            // Check mode
            if (!string.IsNullOrEmpty(Adaptor.StreamInfo.BroadcastIP))
            {
                // Report
                ShowMessage(Properties.Resources.NoMulticastTimeShift, Properties.Resources.Warning_NotAvailable, true);

                // Done
                return;
            }

            // Not possible at this moment
            var current = m_CurrentSource;
            if (current == null)
                return;
            var path = current.PrimaryPath;
            if (string.IsNullOrEmpty(path))
                return;

            // Start it
            Adaptor.StartReplay(path, current.Name, current.Activity);
        }

        /// <summary>
        /// Ermittelt die Liste der verfügbaren Aufzeichnungen.
        /// </summary>
        public override void LoadStations()
        {
            // Index to set default upon
            int startupIndex = -1;

            // Special
            if (m_StartupStation != null)
                if (m_StartupStation.StartsWith("dvbnet5:"))
                {
                    // Get the index
                    startupIndex = int.Parse(m_StartupStation.Substring(7));

                    // No direct default
                    m_StartupStation = null;
                }

            // Reset current channel
            m_DefaultStation = m_StartupStation;
            m_StartupStation = null;

            // Forbid restriction on channels
            Favorites.DisableFavorites();

            // All names already in use
            var duplicates = new Dictionary<string, int>();

            // Find all current activities
            foreach (var activity in VCRNETRestProxy.GetActivitiesForProfile(Adaptor.EndPoint, Profile))
            {
                // Create the information record
                var item = new JobScheduleInfo(activity);
                var name = activity.name;

                // Read counter
                int cnt;
                if (duplicates.TryGetValue(name, out cnt))
                    name = string.Format("{0} ({1})", name, cnt);
                else
                    cnt = 0;

                // Store back
                duplicates[name] = ++cnt;

                // Add to list
                Favorites.AddChannel(name, item);

                // Remember
                if (m_DefaultStation == null)
                    if ((startupIndex < 0) || (startupIndex == activity.streamIndex))
                        m_DefaultStation = name;
            }

            // Finish
            Favorites.FillChannelList();
        }

        /// <summary>
        /// Meldet die erste Teilaufzeichnung der laufenden Aufzeichnung.
        /// </summary>
        public override string DefaultStation { get { return m_DefaultStation; } }

        /// <summary>
        /// NVOD Dienste werden auf diesem Wege grundsätzlich nicht angeboten.
        /// </summary>
        public override ServiceItem[] Services { get { return new ServiceItem[0]; } }

        /// <summary>
        /// Ein NVOD Dienst kann auf diesem Wege nicht ausgewählt werden.
        /// </summary>
        /// <param name="service">Name des gewünschten NVOD Dienstes.</param>
        /// <returns>Wirft immer eine <see cref="NotSupportedException"/>.</returns>
        public override string SetService(ServiceItem service)
        {
            // Not supported
            return null;
        }

        /// <summary>
        /// Wählt eine Aufzeichnung aus.
        /// </summary>
        /// <param name="context">Die gewünschte Aufzeichnung.</param>
        /// <returns>Aktuelle Aufzeichnung samt aktiver Tonspur oder <i>null</i>.</returns>
        public override string SetStation(object context)
        {
            // Stop sending data
            Accessor.Stop();

            // Restart videotext caching from scratch
            Adaptor.VideoText.Deactivate(true);

            // Reset
            Disconnect();

            // Attach to the item
            var item = (JobScheduleInfo)context;

            // Get the signal    
            item.StreamTo(Adaptor.EndPoint, Adaptor.Target);

            // Remember
            m_CurrentSource = item;

            // Process
            return item.ToString();
        }

        /// <summary>
        /// Kappt die Verbindung zur aktuellen Aufzeichnung.
        /// </summary>
        private void Disconnect()
        {
            // Process
            var current = m_CurrentSource;
            if (current != null)
                try
                {
                    // Process
                    current.StreamTo(Adaptor.EndPoint, null);
                }
                catch
                {
                    // Actually server may be already done so ignore any error
                }

            // Forget
            m_CurrentSource = null;
        }

        /// <summary>
        /// Meldet den Namen der aktiven Aufzeichnung.
        /// </summary>
        public override string StationName { get { return (m_CurrentSource == null) ? null : m_CurrentSource.Name; } }

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig.
        /// </summary>
        protected override void OnDispose()
        {
            // Discard
            Disconnect();
        }

        /// <summary>
        /// Trennt vor dem Wechsel des Profils die Verbindung zu einer Aufzeichnung.
        /// </summary>
        public override void OnProfileChanging()
        {
            // Disconnect current stream
            Disconnect();
        }

        /// <summary>
        /// Prüft die Liste der aktiven Aufzeichnungen.
        /// </summary>
        /// <param name="activities">Eine nicht leere Liste von aktiven Auzeichnungen.</param>
        private void ValidateActiveRecording(VCRNETRestProxy.Current[] activities)
        {
            // Check for special operations
            var first = activities[0];
            var current = m_CurrentSource;

            // See if there is a task running
            if (first.streamIndex < 0)
                ShowMessage(Properties.Resources.CurrentUntil, Properties.Resources.Warning_NotAvailable, false, first.source, first.EndsAt.ToLocalTime());

            // See if we have no current source
            else if (current == null)
            {
                // Start watching
                Adaptor.StartWatch(null);

                // Done
                return;
            }

            // Check for current stream
            else
            {
                // Set if we are connected
                var newCurrent = activities.First(activity => activity.referenceId.Value == current.Activity.referenceId.Value);
                if (newCurrent != null)
                    if (string.IsNullOrEmpty(newCurrent.streamTarget))
                    {
                        // Start watching
                        Adaptor.StartWatch(null);

                        // Done
                        return;
                    }
            }

            // Restart request
            VCRNETRestProxy.GetActivities(Adaptor.EndPoint, ReceiveCurrentRecording, null);
        }

        /// <summary>
        /// Prüft, ob in den LIVE Modus gewechselt werden kann.
        /// </summary>
        /// <param name="next">Die als nächstens anstehende Aktivität.</param>
        private void ValidateIdle(VCRNETRestProxy.Current next)
        {
            // Get the next recording
            if (next.start.HasValue)
            {
                // When will it start
                var delta = next.start.Value - DateTime.UtcNow;
                if (delta.TotalMinutes <= 3)
                {
                    // Report
                    ShowMessage(Properties.Resources.NextRecording, Properties.Resources.Warning_NotAvailable, false, (int)delta.TotalSeconds);

                    // Restart request
                    VCRNETRestProxy.GetActivities(Adaptor.EndPoint, ReceiveCurrentRecording, null);

                    // Done
                    return;
                }
            }

            // Can use LIVE
            Adaptor.StartLIVE();
        }

        /// <summary>
        /// Prüft, ob eine Aufzeichnung läuft oder in Kürze beginnt.
        /// </summary>
        public override void KeepAlive()
        {
            // Read results
            var activities = (m_allActivities ?? Enumerable.Empty<VCRNETRestProxy.Current>()).Where(activity => ProfileManager.ProfileNameComparer.Equals(activity.device, Profile)).ToArray();
            var running = activities.Where(activity => activity.IsActive).ToArray();
            var gotResult = m_allActivitiesValid;

            // Prepare for next call
            m_allActivities = null;
            m_allActivitiesValid = false;

            // See if result is available
            if (gotResult)
                if (running.Length > 0)
                    ValidateActiveRecording(running);
                else if (activities.Length > 0)
                    ValidateIdle(activities[0]);
                else
                    Adaptor.StartLIVE();
            else
                VCRNETRestProxy.GetActivities(Adaptor.EndPoint, ReceiveCurrentRecording, null);
        }

        /// <summary>
        /// Meldet, dass bei Verbindung an eine Aufzeichnung Sender und Tonspur nicht
        /// in den Einstellungen zu vermerken sind.
        /// </summary>
        public override bool UpdateSettings { get { return false; } }

        /// <summary>
        /// Die Auswahl von Diensten wird nicht unterstützt.
        /// </summary>
        public override Keys? ServiceListKey { get { return null; } }

        /// <summary>
        /// Eine Aufzeichnung wird nicht unterstützt, da wir bereits mit einer Aufzeichnung verbunden sind.
        /// </summary>
        public override Keys? RecordingKey { get { return null; } }

        /// <summary>
        /// Meldet die Taste zum Umschalten in den TimeShift Modus.
        /// </summary>
        public override Keys? TimeShiftKey { get { return Keys.Multiply; } }
    }
}
