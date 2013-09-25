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
        /// <param name="buffer">Ein Speicherbereich, der zu befüllen ist.</param>
        /// <param name="offset">Die Position des ersten Bytes, das befüllt werden darf.</param>
        /// <param name="length">Die maximale Anzahl von zu befüllenden Bytes.</param>
        /// <returns>Die tatsächlich Anzahl der übertragenen Bytes.</returns>
        int Read( byte[] buffer, int offset, int length );

        /// <summary>
        /// Meldet oder setzt die aktuelle Position im Datenstrom.
        /// </summary>
        long Position { get; set; }

        /// <summary>
        /// Meldet die aktuelle Größe des Datenstroms.
        /// </summary>
        long Length { get; }
    }
}
