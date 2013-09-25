using System;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt ein mögliches Gerät.
    /// </summary>
    public interface IScheduleResource
    {
        /// <summary>
        /// Meldet den Namen der Ressource.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Meldet die absolute Prioriät des Gerätes. Größere Werte veranlassen die Lastverteilung, das
        /// Gerät bevorzugt zu verwenden.
        /// </summary>
        int AbsolutePriority { get; }

        /// <summary>
        /// Beschreibt die Grenzwerte der Entschlüsselung für dieses individuelle Gerät.
        /// </summary>
        DecryptionLimits Decryption { get; }

        /// <summary>
        /// Die maximale Anzahl unterschiedlicher Quellen, die von diesem Geräte gleichzeitig
        /// bearbeitet werden können.
        /// </summary>
        int SourceLimit { get; }

        /// <summary>
        /// Prüft, ob eine bestimmte Quelle über dieses Gerät angesprochen werden kann.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Gesetzt, wenn die Quelle angesprochen werden kann.</returns>
        bool CanAccess( IScheduleSource source );
    }
}
