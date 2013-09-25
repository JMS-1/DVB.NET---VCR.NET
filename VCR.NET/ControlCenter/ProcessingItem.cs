using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;


namespace VCRControlCenter
{
    internal class ProcessingItem
    {
        public readonly string ServerName;
        public readonly ushort ServerPort;
        public readonly bool RunExtensions;

        public PerServerSettings.PerServerView View { get; private set; }
        private VCRNETControl m_Context;

        public ProcessingItem( PerServerSettings.PerServerView view, VCRNETControl context )
        {
            // Remember
            m_Context = context;
            View = view;

            // Attach to settings
            PerServerSettings settings = view.Settings;

            // Create a snap shot
            ServerName = settings.ServerName;
            ServerPort = settings.ServerPort;
            RunExtensions = settings.RunExtensions;

            // Report
            View.StartProcessing();
        }

        /// <summary>
        /// Meldet die Adresse des VCR.NET Recording Service
        /// </summary>
        public string EndPoint { get { return string.Format( "http://{0}:{1}/VCR.NET", ServerName, ServerPort ); } }

        /// <summary>
        /// Wird periodisch aufgerufen und frage den aktuellen Zustand des Dienstes auf einem Rechner ab.
        /// </summary>
        /// <param name="state">Wird ignoriert.</param>
        public void CheckServer( object state )
        {
            // Create a processing clone
            var settings = new VCRNETRestProxy.ServiceInformation();
            var serverInfo = View.ServerInfo;

            // Safe process
            try
            {
                // Report
                VCRNETControl.Log( "Checking {0}:{1}", ServerName, ServerPort );

                // Read the server settings
                settings = VCRNETRestProxy.GetInformation( EndPoint );

                // Load version once
                if (string.IsNullOrEmpty( serverInfo.Version ))
                    serverInfo.Version = settings.version;

                // Check mode
                bool hasNext = false, near = false;

                // Find all activities
                var activities = VCRNETRestProxy.GetActivities( EndPoint );

                // All profiles
                foreach (var profile in settings.profileNames)
                {
                    // Report
                    VCRNETControl.Log( "Processing Profile {2} for {0}:{1}", ServerName, ServerPort, profile );

                    // Attach to the profile
                    var info = serverInfo[profile];
                    var profileActivities = activities.Where( activity => StringComparer.InvariantCultureIgnoreCase.Equals( activity.device, profile ) ).ToArray();
                    var profileCurrent = profileActivities.Where( activity => activity.IsActive ).ToArray();

                    // Check for a current recording
                    if (profileCurrent.Length > 0)
                    {
                        // Remember
                        info.CurrentRecordings = profileCurrent;

                        // Done
                        continue;
                    }

                    // Find the next recording
                    var next = profileActivities.FirstOrDefault();
                    if (next == null)
                        continue;
                    if (!next.start.HasValue)
                        continue;

                    // At least there are recordings
                    hasNext = true;

                    // Check delta
                    var delta = next.start.Value - DateTime.UtcNow;
                    if (delta.TotalMinutes < Math.Max( 5, settings.sleepMinimum ))
                        near = true;
                }

                // Check mode
                if (serverInfo.State == TrayColors.Red)
                    if (near)
                        serverInfo.State = TrayColors.Yellow;
                    else if (hasNext)
                        serverInfo.State = TrayColors.Green;
                    else
                        serverInfo.State = TrayColors.Standard;
            }
            catch
            {
            }

            // Update
            View.ServerInfo = serverInfo;

            // Report
            if (!m_Context.ProcessStateAndCheckHibernation( this, serverInfo.State, settings.hibernationPending, View.Settings.IsLocal && settings.extensionsRunning ))
                return;

            // Safe tell the server that we take over hibernation control.
            try
            {
                // Process
                VCRNETRestProxy.ResetPendingHibernation( EndPoint );
            }
            catch
            {
            }
        }
    }
}
