using System.Threading;
using JMS.DVB.SI;


namespace JMS.DVB
{
    /// <summary>
    /// Hilfsklasse zum Einlesen einer einzelnen Tabelle, die auf mehrere SI Bereiche verteilt
    /// sein kann.
    /// </summary>
    public static class TableReader
    {
        /// <summary>
        /// Startet das Auslesen einer SI-Tabelle.
        /// </summary>
        /// <typeparam name="TTableType">Die Art der Tabelle.</typeparam>
        /// <param name="device">Das zu verwendende, bereits aktivierte Gerät.</param>
        /// <returns>Die Steuerung des Auslesevorgangs.</returns>
        public static CancellableTask<TTableType[]> GetTableAsync<TTableType>( this Hardware device ) where TTableType : WellKnownTable
        {
            return device.GetTableAsync<TTableType>( WellKnownTable.GetWellKnownStream<TTableType>() );
        }

        /// <summary>
        /// Erzeugt eine neue Hintergrundaufgabe zum Auslesen von SI Tabellen.
        /// </summary>
        /// <param name="device">Das zu verwendende Gerät.</param>
        /// <param name="stream">Die Datenstromkennung, die überwacht werden soll.</param>
        /// <returns>Die neue Aufgabe.</returns>
        public static CancellableTask<TTableType[]> GetTableAsync<TTableType>( this Hardware device, ushort stream ) where TTableType : Table
        {
            // Create the task
            return CancellableTask<TTableType[]>.Run( cancel =>
            {
                // Prepare
                var tables = default( TTableType[] );
                var sync = new object();
                var expectedVersion = 0;
                var collectedParts = 0;
                var done = false;

                // Create parser
                var parser = TableParser.Create( ( TTableType table ) =>
                {
                    // Disabled
                    if (done)
                        return;

                    // Check version
                    if (tables != null)
                        if (table.Version != expectedVersion)
                            tables = null;

                    // Discard on count mismatch
                    if (tables != null)
                        if (tables.Length != (table.LastSection + 1))
                            tables = null;

                    // Discard on overrun
                    if (tables != null)
                        if (table.CurrentSection >= tables.Length)
                            tables = null;

                    // Discard on duplicates
                    if (tables != null)
                        if (tables[table.CurrentSection] != null)
                            tables = null;

                    // Create once
                    if (tables == null)
                    {
                        // Create in full size
                        tables = new TTableType[table.LastSection + 1];
                        expectedVersion = table.Version;
                        collectedParts = 0;
                    }

                    // Add it
                    tables[table.CurrentSection] = table;

                    // Mark as done
                    if (++collectedParts >= tables.Length)
                        lock (sync)
                        {
                            // Mark
                            done = true;

                            // Signal
                            Monitor.Pulse( sync );
                        }
                } );

                // Register with cleanup
                var tableType = Table.GetIsExtendedTable<TTableType>() ? StreamTypes.ExtendedTable : StreamTypes.StandardTable;
                var registration = device.AddConsumer( stream, parser, tableType );
                try
                {
                    // Start receiving data
                    device.SetConsumerState( registration, true );

                    // Wait for end   
                    using (cancel.Register( () => { lock (sync) Monitor.Pulse( sync ); } ))
                        lock (sync)
                            while (!done)
                                if (cancel.IsCancellationRequested)                                    
                                    return null;
                                else
                                    Monitor.Wait( sync );
                }
                finally
                {
                    // Cleanup
                    device.SetConsumerState( registration, null );
                }

                // Report
                return tables;
            } );
        }
    }
}
