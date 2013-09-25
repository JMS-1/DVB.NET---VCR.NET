using System;
using System.Data;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;

namespace JMS.DVB
{
    /// <summary>
    /// Zeigt einen Dialog zur Pflege des DVB.NET Benutzerprofils.
    /// </summary>
    internal partial class UserProfileManager : Form
    {
        /// <summary>
        /// Ein Eintrag für die Auswahl einer Sprache.
        /// </summary>
        private class CultureItem
        {
            /// <summary>
            /// Die Sprachinformationen.
            /// </summary>
            public CultureInfo Info { get; private set; }

            /// <summary>
            /// Erzeugt einen neuen Auswahleintrag.
            /// </summary>
            /// <param name="info"></param>
            public CultureItem( CultureInfo info )
            {
                // Remember
                Info = info;
            }

            /// <summary>
            /// Zeigt den Namen der Sprache an.
            /// </summary>
            /// <returns>Der Name der Sprache in der jeweiligen Sprache.</returns>
            public override string ToString()
            {
                // Report
                return Info.NativeName;
            }
        }

        /// <summary>
        /// Erzeugt einen neuen Dialog.
        /// </summary>
        public UserProfileManager()
        {
            // Set me up
            InitializeComponent();

            // Fill
            ckAllways.Checked = UserProfile.RawAlwaysShowDialog;
        }

        /// <summary>
        /// Initialisiert den Dialog.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void UserProfileManager_Load( object sender, EventArgs e )
        {
            // The default
            CultureItem selectedItem = null;

            // Load the preferred language
            string preferred = UserProfile.RawLanguage;

            // Fill language list
            foreach (CultureInfo info in CultureInfo.GetCultures( CultureTypes.NeutralCultures ))
                if (info.NativeName.IndexOf( '(' ) < 0)
                {
                    // Create
                    CultureItem item = new CultureItem( info );

                    // Add to map
                    selLanguage.Items.Add( item );

                    // Remember default
                    if (0 == string.Compare( preferred, info.TwoLetterISOLanguageName, true ))
                        selectedItem = item;
                }

            // Copy over
            selLanguage.SelectedItem = selectedItem;

            // Load all profiles
            LoadProfiles();
        }

        /// <summary>
        /// Lädt die Namen aller Geräteprofile.
        /// </summary>
        private void LoadProfiles()
        {
            // Reset list
            selProfile.Items.Clear();

            // Load all
            selProfile.Items.AddRange( ProfileManager.AllProfiles );

            // No profile selected so far
            if (!string.IsNullOrEmpty( UserProfile.RawProfileName ))
            {
                // Load it
                Profile profile = ProfileManager.FindProfile( UserProfile.RawProfileName );

                // Select it
                if (null != profile)
                    selProfile.SelectedItem = profile;
            }

            // Report to user instance
            UpdateSaveButton( selProfile, EventArgs.Empty );
        }

        /// <summary>
        /// Aktualisiert die Schaltfläche zum Speichern.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void UpdateSaveButton( object sender, EventArgs e )
        {
            // Enable button
            cmdSave.Enabled = ((null != selProfile.SelectedItem) || (null != selLanguage.SelectedItem));
        }

        /// <summary>
        /// Speichert das Benutzerprofil.
        /// </summary>
        /// <param name="sender">Wird ignoriert.</param>
        /// <param name="e">Wird ignoriert.</param>
        private void cmdSave_Click( object sender, EventArgs e )
        {
            // Load selections
            CultureItem culture = (CultureItem) selLanguage.SelectedItem;
            Profile profile = (Profile) selProfile.SelectedItem;

            // Update the all
            UserProfile.RawLanguage = (null == culture) ? null : culture.Info.TwoLetterISOLanguageName;
            UserProfile.RawProfileName = (null == profile) ? null : profile.Name;
            UserProfile.RawAlwaysShowDialog = ckAllways.Checked;

            // Store
            UserProfile.Save();
        }
    }
}