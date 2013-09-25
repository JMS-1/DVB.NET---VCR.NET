using System;

using JMS.DVB.DeviceAccess.Enumerators;


namespace DVBNETViewer.Dialogs
{
    /// <summary>
    /// Bietet einen Filter zur Auswahl durch den Anwender an.
    /// </summary>
    internal class DecoderItem
    {
        /// <summary>
        /// Die zugehörigen Informationen.
        /// </summary>
        public IDeviceOrFilterInformation Information { get; private set; }

        /// <summary>
        /// Legt eine Auswahl an.
        /// </summary>
        /// <param name="information">Die zugehörigen Informationen.</param>
        public DecoderItem( IDeviceOrFilterInformation information )
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
    }
}
