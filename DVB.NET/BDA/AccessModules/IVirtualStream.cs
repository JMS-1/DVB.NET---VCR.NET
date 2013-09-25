using System;


namespace JMS.DVB.DirectShow.AccessModules
{
    /// <summary>
    /// Beschreibt die Minimalanforderungen zur Anzeige eines Datenstroms.
    /// </summary>
    public interface IVirtualStream : IDisposable
    {
        /// <summary>
        /// Meldet den Namen dieses Datenstroms.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Liest Daten ab der aktuellen Position.
        /// </summary>
        /// <param name="buffer">Ein Speicherbereich, der zu bef�llen ist.</param>
        /// <param name="offset">Die Position des ersten Bytes, das bef�llt werden darf.</param>
        /// <param name="length">Die maximale Anzahl von zu bef�llenden Bytes.</param>
        /// <returns>Die tats�chlich Anzahl der �bertragenen Bytes.</returns>
        int Read( byte[] buffer, int offset, int length );

        /// <summary>
        /// Meldet oder setzt die aktuelle Position im Datenstrom.
        /// </summary>
        long Position { get; set; }

        /// <summary>
        /// Meldet die aktuelle Gr��e des Datenstroms.
        /// </summary>
        long Length { get; }
    }
}
