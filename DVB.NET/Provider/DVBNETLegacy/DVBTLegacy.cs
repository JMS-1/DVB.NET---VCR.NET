﻿using JMS.DVB.DeviceAccess.Interfaces;


namespace JMS.DVB.Provider.Legacy
{
    /// <summary>
    /// Diese Klasse vermittelt den Zugriff auf eine vorhandene DVB.NET Abstraktion vor
    /// Version 3.5.1.
    /// </summary>
    public class DVBTLegacy : LegacyHardware<TerrestrialProfile, TerrestrialLocation, TerrestrialGroup>
    {
        /// <summary>
        /// Erzeugt eine neue Vermittlungsinstanz.
        /// </summary>
        /// <param name="profile">Das zugeordnete Geräteprofil.</param>
        public DVBTLegacy( TerrestrialProfile profile )
            : base( profile )
        {
        }

        /// <summary>
        /// Meldet die Art des DVB Empfangs.
        /// </summary>
        protected override DVBSystemType SystemType { get { return DVBSystemType.Terrestrial; } }
    }
}
