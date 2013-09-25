using System;
using System.Linq;
using System.Windows.Forms;

using JMS.DVB.DeviceAccess.Enumerators;


namespace JMS.DVB.Editors
{
    /// <summary>
    /// Erlaubt dem Anwender die Auswahl eines Gerätes.
    /// </summary>
    public class DeviceSelector
    {
        /// <summary>
        /// Die Beschreibung des zugehörigen Gerätes.
        /// </summary>
        public IDeviceOrFilterInformation Information { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Auswahl.
        /// </summary>
        /// <param name="information">Das zugehörige Gerät.</param>
        public DeviceSelector( IDeviceOrFilterInformation information )
        {
            // Remember
            Information = information;
        }

        /// <summary>
        /// Erzeugt den Auswahltext für den Anwender.
        /// </summary>
        /// <returns>Der gewünschte Auswahltext.</returns>
        public override string ToString()
        {
            // Report
            return Information.DisplayName;
        }

        /// <summary>
        /// Aktiviert eine Geräteauswahl.
        /// </summary>
        /// <param name="selector">Die zu verwendende Auswahlliste.</param>
        /// <param name="displayName">Der zugehörige Anzeigename.</param>
        /// <param name="moniker">Der zugehörige eindeutige Name,</param>
        public static void Select( ComboBox selector, string displayName, string moniker )
        {
            // Process all - by moniker
            foreach (var item in selector.Items.OfType<DeviceSelector>())
                if (Equals( item.Information.UniqueName, moniker ))
                {
                    // Select
                    selector.SelectedItem = item;

                    // Done
                    return;
                }

            // Process all - by display
            foreach (var item in selector.Items.OfType<DeviceSelector>())
                if (Equals( item.Information.DisplayName, displayName ))
                {
                    // Select
                    selector.SelectedItem = item;

                    // Done
                    return;
                }
        }
    }
}
