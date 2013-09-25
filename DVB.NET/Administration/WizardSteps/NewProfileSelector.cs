using System;
using JMS.DVB;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using JMS.DVB.Administration;
using System.Collections.Generic;

namespace DVBNETAdmin.WizardSteps
{
    /// <summary>
    /// Wählt ein einzelnes, neues Geräteprofil aus.
    /// </summary>
    public partial class NewProfileSelector : Step
    {
        /// <summary>
        /// Erzeugt eine neue Auswahl
        /// </summary>
        public NewProfileSelector()
        {
            // Overwerite designer settings
            InitializeComponent();
        }

        /// <summary>
        /// Meldet die Überschrift für diesen Arbeitsschritt.
        /// </summary>
        public override string Headline
        {
            get
            {
                // Report
                return Properties.Resources.Navigation_Select1Profile;
            }
        }

        /// <summary>
        /// Setzt oder liest das aktuell ausgewählte Geräteprofil.
        /// </summary>
        public string CurrentProfile
        {
            get
            {
                // Report
                return GetState<string>();
            }
            set
            {
                // Update
                SetState( value );
            }
        }

        /// <summary>
        /// Prüft, ob für die ausgewählte Aufgabe mindestens ein Geräteprofil angeboten werden kann.
        /// </summary>
        /// <param name="plugIn">Die zu prüfende Erweiterung.</param>
        /// <returns>Gesetzt, wenn die Erweiterung mindestens ein zulässiges Geräteprofile benötigt.</returns>
        public static bool IsValidFor( PlugIn plugIn )
        {
            // None at all
            if (plugIn.MaximumNewProfiles < 1)
                return false;

            // Get all profiles
            List<Profile> profiles = new List<Profile>( ProfileManager.AllProfiles );

            // Ask extension to clean up the list
            plugIn.FilterNewProfiles( profiles );

            // Check limit
            if (plugIn.MaximumNewProfiles > 1)
                return (1 == profiles.Count);
            else
                return (0 != profiles.Count);
        }

        /// <summary>
        /// Füllt die Eingabeelemente dieses Arbeitsschritts.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void NewProfileSelector_Load( object sender, EventArgs e )
        {
            // Load all profiles
            List<Profile> profiles = new List<Profile>( ProfileManager.AllProfiles );

            // Restrict by plug-in
            MainForm.CurrentPlugIn.FilterNewProfiles( profiles );

            // Sort
            profiles.Sort( ( l, r ) => l.Name.CompareTo( r.Name ) );

            // Find the selection
            string selected = CurrentProfile;

            // Add starter
            selProfile.Items.Add( new ProfileItem { IsRequired = (MainForm.CurrentPlugIn.MinimumNewProfiles > 0) } );

            // Select this
            selProfile.SelectedIndex = 0;

            // Convert
            List<ProfileItem> items = profiles.ConvertAll( p => new ProfileItem { Profile = p } );

            // Add the rest
            selProfile.Items.AddRange( items.ToArray() );

            // Special
            if (2 == selProfile.Items.Count)
            {
                // Get rid of the special selection
                selProfile.Items.RemoveAt( 0 );

                // Select the only one
                selProfile.SelectedIndex = 0;

                // Disable selection
                selProfile.Enabled = false;

                // Fire event
                selProfile_SelectionChangeCommitted( selProfile, EventArgs.Empty );
            }
            else
            {
                // See if selection exists
                ProfileItem item = items.FirstOrDefault( i => (null != i.Profile) && (0 == string.Compare( i.Profile.Name, selected, true )) );

                // Select
                if (null != item)
                {
                    // Update
                    selProfile.SelectedItem = item;

                    // Fire event
                    selProfile_SelectionChangeCommitted( selProfile, EventArgs.Empty );
                }
            }

            // May forward automatically
            MainForm.ProcessAutoForward();
        }

        /// <summary>
        /// Es wurde ein Geräteprofil ausgewählt.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void selProfile_SelectionChangeCommitted( object sender, EventArgs e )
        {
            // Load selection
            ProfileItem item = (ProfileItem) selProfile.SelectedItem;

            // Update state
            CurrentProfile = (null == item.Profile) ? null : item.Profile.Name;

            // Update master
            CheckButtons();
        }

        /// <summary>
        /// Meldet, ob der nächste Arbeitsschritt ausgelöst werden darf.
        /// </summary>
        public override bool EnableNext
        {
            get
            {
                // As soon as profile is selected
                return (null != CurrentProfile);
            }
        }

        /// <summary>
        /// Führt den nächsten Arbeitsschritt aus.
        /// </summary>
        public override void NextStep()
        {
            // Fixed
            MainForm.ShowStep<PlugInExecutor>();
        }
    }
}
