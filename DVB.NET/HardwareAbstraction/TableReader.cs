using System;
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
            // Validate
            if (device == null)
                throw new ArgumentException( "no hardware to use", "device" );

            // Create the task
            return
                CancellableTask<TTableType[]>.Run( cancel =>
                {
                    // Termination synchronisation
                    var sync = new object();
                    var done = false;

                    // Parsing state
                    var tables = default( TTableType[] );
                    var expectedVersion = 0;
                    var collectedParts = 0;

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
                                done = true;

                                // Our wake up call
                                Monitor.Pulse( sync );
                            }
                    } );

                    // Register with cleanup
                    var tableType = Table.GetIsExtendedTable<TTableType>() ? StreamTypes.ExtendedTable : StreamTypes.StandardTable;
                    var registration = device.AddConsumer( stream, tableType, parser.AddPayload );
                    try
                    {
                        // Start receiving data
                        device.SetConsumerState( registration, true );

                        // Wait for end and fully process the cancellation token
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
