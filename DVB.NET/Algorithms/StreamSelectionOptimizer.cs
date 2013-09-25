using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace JMS.DVB.Algorithms
{
    /// <summary>
    /// Bestimmt, welche Aspekte einer Quelle ausgeblendet werden dürfen.
    /// </summary>
    public enum StreamDisableSelector
    {
        /// <summary>
        /// DVB Untertitel der eigenen Quelle.
        /// </summary>
        SubTitlesSelf,

        /// <summary>
        /// DVB Untertitel höher priorisierter Quellen.
        /// </summary>
        SubTitlesHigher,

        /// <summary>
        /// DVB Untertitel niedriger priorisierter Quellen.
        /// </summary>
        SubTitlesLower,

        /// <summary>
        /// Alternative MP2 Tonspuren der eigenen Quelle.
        /// </summary>
        AlternateMP2Self,

        /// <summary>
        /// Alternative MP2 Tonspuren höher priorisierter Quellen.
        /// </summary>
        AlternateMP2Higher,

        /// <summary>
        /// Alternative MP2 Tonspuren niedriger priorisierter Quellen.
        /// </summary>
        AlternateMP2Lower,

        /// <summary>
        /// Alternative AC3 Tonspuren der eigenen Quelle.
        /// </summary>
        AlternateAC3Self,

        /// <summary>
        /// Alternative AC3 Tonspuren höher priorisierter Quellen.
        /// </summary>
        AlternateAC3Higher,

        /// <summary>
        /// Alternative AC3 Tonspuren niedriger priorisierter Quellen.
        /// </summary>
        AlternateAC3Lower,

        /// <summary>
        /// Alle AC3 Tonspuren der eigenen Quelle, wenn mindestens eine MP2 Tonspur existiert.
        /// </summary>
        AC3IfMP2ExistsSelf,

        /// <summary>
        /// Alle AC3 Tonspuren höher priorisierter Quellen, wenn mindestens eine MP2 Tonspur verbleibt.
        /// </summary>
        AC3IfMP2ExistsHigher,

        /// <summary>
        /// Alle AC3 Tonspuren niedriger priorisierter Quellen, wenn mindestens eine MP2 Tonspur verbleibt.
        /// </summary>
        AC3IfMP2ExistsLower,

        /// <summary>
        /// Videotext der eigenen Quelle.
        /// </summary>
        VideoTextSelf,

        /// <summary>
        /// Videotext höher priorisierter Quellen.
        /// </summary>
        VideoTextHigher,

        /// <summary>
        /// Videotext niedriger priorisierter Quellen.
        /// </summary>
        VideoTextLower,

        /// <summary>
        /// Alle MP2 Tonspuren der eigenen Quelle, wenn mindestens eine AC3 Tonspur existiert.
        /// </summary>
        MP2IfAC3ExistsSelf,

        /// <summary>
        /// Alle MP2 Tonspuren höher priorisierter Quellen, wenn mindestens eine AC3 Tonspur verbleibt.
        /// </summary>
        MP2IfAC3ExistsHigher,

        /// <summary>
        /// Alle MP2 Tonspuren niedriger priorisierter Quellen, wenn mindestens eine AC3 Tonspur verbleibt.
        /// </summary>
        MP2IfAC3ExistsLower
    }

    /// <summary>
    /// Beschreibt, welche Art von Auswahl gerade bearbeitet wird.
    /// </summary>
    internal enum StreamDisableMode
    {
        /// <summary>
        /// Die Korrektur bezieht sich auf die Quelle selbst.
        /// </summary>
        Self,

        /// <summary>
        /// Die Korrektur wird auf eine höher priorisierte Quelle angewendet.
        /// </summary>
        Higher,

        /// <summary>
        /// Die Korrektur wird auf eine niedriger priorisierte Quelle angewendet.
        /// </summary>
        Lower
    }

    /// <summary>
    /// Diese Klasse wird verwendet, um eine Gruppe von gleichzeitigen Aufzeichnungen
    /// vorzubereiten. Dabei sind besondere Maßnahmen notwendig, wenn die verwendete 
    /// DVB.NET Hardware nur eine begrenzte Anzahl von Verbrauchern erlaubt.
    /// </summary>
    public class StreamSelectionOptimizer
    {
        /// <summary>
        /// Kann aktiviert werden um die Arbeit der Optimierung zu beobachten.
        /// </summary>
        public static readonly BooleanSwitch OptimizerTraceSwitch = new BooleanSwitch( "StreamOptimizerTrace", "Shows which Streams have to been removed due to Consumer Limitations" );

        /// <summary>
        /// Die Reihenfolge, in der Aspekte beim Empfang ausgeblendet werden dürfen.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Grundsätzlich werden erst einmal die niedriger priorisierten Quellen reduziert,
        /// beginnend mit der jeweils als letztes hinzugefügten. Dann die Quelle, die nicht
        /// mehr vollständig gestartet werden konnte, gefolgt von den höher priorisierten
        /// Quellen.
        /// </para>
        /// <para>
        /// Als grundsätzliche Voreinstellung wurde gewählt: DVB Untertitel, alternative AC3
        /// Tonspuren, alle AC3 Tonspuren bei vorhandenen MP2 Tonspuren, alternative MP2
        /// Tonspuren, alle MP2 Tonspuren bei vorhandenen AC3 Tonspuren und zuletzt der
        /// Videotext. Es ist damit sichergestellt, dass zum Bild immer mindestens eine
        /// Tonspur (in der Voreinstellung MP2, falls AC3 und MP2 vorhanden sind) mit
        /// empfangen wird.
        /// </para>
        /// </remarks>
        public static readonly StreamDisableSelector[] DefaultDisableOrder = 
            {
                StreamDisableSelector.SubTitlesLower,
                StreamDisableSelector.SubTitlesSelf,
                StreamDisableSelector.SubTitlesHigher,
                StreamDisableSelector.AlternateAC3Lower,
                StreamDisableSelector.AlternateAC3Self,
                StreamDisableSelector.AlternateAC3Higher,
                StreamDisableSelector.AC3IfMP2ExistsLower,
                StreamDisableSelector.AC3IfMP2ExistsSelf,
                StreamDisableSelector.AC3IfMP2ExistsHigher,
                StreamDisableSelector.AlternateMP2Lower,
                StreamDisableSelector.AlternateMP2Self,
                StreamDisableSelector.AlternateMP2Higher,
                StreamDisableSelector.MP2IfAC3ExistsLower,
                StreamDisableSelector.MP2IfAC3ExistsSelf,
                StreamDisableSelector.MP2IfAC3ExistsHigher,
                StreamDisableSelector.VideoTextLower,
                StreamDisableSelector.VideoTextSelf,
                StreamDisableSelector.VideoTextHigher,
            };

        /// <summary>
        /// Beschreibt eine einzelne Aufzeichnung.
        /// </summary>
        private class SelectionInfo
        {
            /// <summary>
            /// Die verwendete Quelle.
            /// </summary>
            public SourceSelection Source { get; set; }

            /// <summary>
            /// Die aktuell verwendeten Datenströme.
            /// </summary>
            public StreamSelection CurrentStreams { get; set; }

            /// <summary>
            /// Die ursprünglich verwendeten Datenströme.
            /// </summary>
            public StreamSelection OriginalStreams { get; set; }

            /// <summary>
            /// Die Anzahl der benötigten Teildatenströme (PID).
            /// </summary>
            public int? ConsumerCount { get; set; }

            /// <summary>
            /// Erzeugt eine neue Beschreibung.
            /// </summary>
            public SelectionInfo()
            {
            }

            /// <summary>
            /// Versucht, möglichst viele der gewünschten Aspekte dieser Quelle zu aktivieren.
            /// </summary>
            /// <param name="disableOrder">Die Reihenfolge, in der Aspekte deaktiviert werden
            /// dürfen.</param>
            /// <param name="report">Protokolliert die vorgenommenen Veränderungen.</param>
            public void Optimize( StreamDisableSelector[] disableOrder, Action<SourceSelection, string, int> report )
            {
                // Already did it
                if (ConsumerCount.HasValue)
                    return;

                // Number of consumers possible at all
                int available = 0;

                // Try to open
                try
                {
                    // Create the manager
                    using (SourceStreamsManager manager = Source.Open( OriginalStreams ))
                        if (!manager.CreateStream( null ))
                        {
                            // Fake entry
                            CurrentStreams = OriginalStreams.Clone();
                            ConsumerCount = 0;
                        }
                        else
                        {
                            // Remember
                            CurrentStreams = manager.ActiveSelection;
                            ConsumerCount = manager.ConsumerCount;
                        }

                    // Possible at least in stand-alone mode
                    return;
                }
                catch (OutOfConsumersException e)
                {
                    // Remember
                    CurrentStreams = e.RequestedSelection;
                    ConsumerCount = e.Requested;
                    available = e.Available;
                }

                // What we need
                int needed = ConsumerCount.Value - available;

                // Process all modes
                for (int i = 0; (needed > 0) && (i < disableOrder.Length); ++i)
                    ApplyDisable( disableOrder[i], StreamDisableMode.Self, ref needed, report );
            }

            /// <summary>
            /// Wendet eine Ausschlußoption an.
            /// </summary>
            /// <param name="disable">Die gewünschte Option.</param>
            /// <param name="mode">Der aktuelle Bearbeitungsmodus.</param>
            /// <param name="needed">Die Anzahl der noch benötigten Verbraucher, bei Bedarf korrigiert.</param>
            /// <param name="report">Protokolliert die vorgenommenen Veränderungen.</param>
            public void ApplyDisable( StreamDisableSelector disable, StreamDisableMode mode, ref int needed, Action<SourceSelection, string, int> report )
            {
                // Initial value
                int neededStart = needed;

                // Process
                if (!CurrentStreams.ApplyDisable( disable, mode, ref needed, ( m, c ) => { if (null != report) report( Source, m, c ); } ))
                    return;

                // Remember corrected settings
                OriginalStreams = CurrentStreams;

                // Correct consumer count
                ConsumerCount = ConsumerCount.Value - (neededStart - needed);
            }
        }

        /// <summary>
        /// Die Liste der einzelnen Aufzeichnungen.
        /// </summary>
        private List<SelectionInfo> m_Sources = new List<SelectionInfo>();

        /// <summary>
        /// An diese Stelle werden alle vorgenommenen Veränderungen protokolliert.
        /// </summary>
        public event Action<SourceSelection, string, int> OnCorrect;

        /// <summary>
        /// Erzeugt eine neue Hilfsklasse.
        /// </summary>
        public StreamSelectionOptimizer()
        {
        }

        /// <summary>
        /// Meldet eine vorgenommene Veränderung an der Konfiguration.
        /// </summary>
        /// <param name="source">Die Quelle, deren Konfiguration gerade angepasst wird.</param>
        /// <param name="item">Informationen zur Veränderung.</param>
        /// <param name="count">Die Anzahl der eingesparten Verbraucher.</param>
        private void Report( SourceSelection source, string item, int count )
        {
            // Report
            if (OptimizerTraceSwitch.Enabled)
                Trace.WriteLine( string.Format( Properties.Resources.Trace_Optimizer_Report, source.DisplayName, item, count, source.Source ), OptimizerTraceSwitch.DisplayName );

            // Forward
            if (null != OnCorrect)
                OnCorrect( source, item, count );
        }

        /// <summary>
        /// Fügt eine Quelle zur Aufzeichnung hinzu.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <param name="streams">Die Informationen zu den einzuschliessenden
        /// Teildatenströmen (PID).</param>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        public void Add( SourceSelection source, StreamSelection streams )
        {
            // Validate
            if (null == source)
                throw new ArgumentNullException( "source" );
            if (null == streams)
                throw new ArgumentNullException( "streams" );

            // Remember
            m_Sources.Add( new SelectionInfo { Source = source, OriginalStreams = streams.Clone() } );
        }

        /// <summary>
        /// Meldet die Anzahl der Quellen.
        /// </summary>
        public int Count
        {
            get
            {
                // Report
                return m_Sources.Count;
            }
        }

        /// <summary>
        /// Meldet eine der Quellen.
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer der Quelle.</param>
        /// <returns>Die gewünschte Quelle.</returns>
        public SourceSelection GetSource( int index )
        {
            // Forward
            return m_Sources[index].Source;
        }

        /// <summary>
        /// Meldet die tatsächlich verfügbaren Teildatenströme (PID).
        /// </summary>
        /// <param name="index">Die 0-basierte laufende Nummer der Quelle.</param>
        /// <returns>Die berechnete oder originale Auswahl der Teildatenströme.</returns>
        public StreamSelection GetStreams( int index )
        {
            // Attach to the item
            SelectionInfo info = m_Sources[index];

            // Forward
            if (null == info.CurrentStreams)
                return info.OriginalStreams;
            else
                return info.CurrentStreams;
        }

        /// <summary>
        /// Sucht eine Konfiguration der Teildatenströme, die eine Aufzeichnung aller Quellen
        /// eventuell mit Reduktion des Aufzeichnungsumfangs erlaubt.
        /// </summary>
        /// <returns>Die Anzahl der Quellen, die verwendet werden können.</returns>
        public int Optimize()
        {
            // Forward
            return Optimize( DefaultDisableOrder );
        }

        /// <summary>
        /// Sucht eine Konfiguration der Teildatenströme, die eine Aufzeichnung aller Quellen
        /// eventuell mit Reduktion des Aufzeichnungsumfangs erlaubt.
        /// </summary>
        /// <param name="disableOrder">Die Liste der Aspekte, die ausgeblendet werden dürfen.</param>
        /// <returns>Die Anzahl der Quellen, die verwendet werden können.</returns>
        public int Optimize( params StreamDisableSelector[] disableOrder )
        {
            // No souces
            if (m_Sources.Count < 1)
                return m_Sources.Count;

            // Total stream count
            int available;

            // No limit
            using (HardwareManager.Open())
            {
                // Attach to the device
                Hardware device = m_Sources[0].Source.GetHardware();

                // No limit at all
                if (!device.HasConsumerRestriction)
                    return m_Sources.Count;

                // Get all the active streams
                ushort[] activeStreams = device.GetActiveStreams();

                // Ask for it
                available = device.Restrictions.ConsumerLimit.Value - activeStreams.Length;

                // None at all
                if (available < 1)
                    return 0;

                // Stream managers in use
                List<SourceStreamsManager> managers = new List<SourceStreamsManager>();
                try
                {
                    // Create one by one
                    for (int i = 0; i < m_Sources.Count; ++i)
                    {
                        // Attach
                        SelectionInfo info = m_Sources[i];

                        // Create new manager
                        SourceStreamsManager manager = info.Source.Open( info.OriginalStreams );

                        // Remember for cleanupo
                        managers.Add( manager );

                        // See if source is available
                        if (!manager.CreateStream( null ))
                        {
                            // Fake entry - will not be used
                            info.CurrentStreams = info.OriginalStreams.Clone();
                            info.ConsumerCount = 0;
                        }
                        else
                        {
                            // Remember all we found
                            info.CurrentStreams = manager.ActiveSelection;
                            info.ConsumerCount = manager.ConsumerCount;
                        }
                    }

                    // Whoo - can open it all as requested
                    return m_Sources.Count;
                }
                catch (OutOfConsumersException)
                {
                }
                finally
                {
                    // Terminate all
                    foreach (SourceStreamsManager manager in managers)
                        manager.Dispose();
                }

                // First try to make sure that each source can be opened in stand-alone mode
                foreach (SelectionInfo info in m_Sources)
                    info.Optimize( disableOrder, Report );

                // Now simulate starting all - will try to get most out of the first one and so on
                for (int ixStream = 0; ixStream < m_Sources.Count; ixStream++)
                {
                    // Attach to the item
                    SelectionInfo current = m_Sources[ixStream];

                    // See how many additional streams will be needed
                    int needed = current.ConsumerCount.Value - available;

                    // Try to free some
                    for (int ixDisable = 0; (needed > 0) && (ixDisable < disableOrder.Length); )
                    {
                        // Load the next option
                        StreamDisableSelector disable = disableOrder[ixDisable++];

                        // Apply self
                        current.ApplyDisable( disable, StreamDisableMode.Self, ref needed, Report );

                        // Apply higher priorized
                        for (int ixHigher = ixStream; (needed > 0) && (ixHigher-- > 0); )
                            m_Sources[ixHigher].ApplyDisable( disable, StreamDisableMode.Higher, ref needed, Report );
                    }

                    // Should not be startet - we find no way to provide a proper subset of streams
                    if (needed > 0)
                        return ixStream;

                    // Back to what is left
                    available = -needed;
                }

                // Not possible
                return m_Sources.Count;
            }
        }
    }

    /// <summary>
    /// Diverse Hilfsklassen zur Reduktion der Anforderungen an Aspekte einer Aufzeichnung.
    /// </summary>
    internal static class OptimízeExtensions
    {
        /// <summary>
        /// Meldet, ob eine Deaktivierungsoption sich auf eine Auswahl selbst bezieht.
        /// </summary>
        /// <param name="selector">Die Option.</param>
        /// <returns>Gesetzt, wenn die Prüfung erfolgreich war.</returns>
        public static bool IsSelf( this StreamDisableSelector selector )
        {
            // Check it
            if (StreamDisableSelector.AC3IfMP2ExistsSelf == selector)
                return true;
            if (StreamDisableSelector.AlternateAC3Self == selector)
                return true;
            if (StreamDisableSelector.AlternateMP2Self == selector)
                return true;
            if (StreamDisableSelector.MP2IfAC3ExistsSelf == selector)
                return true;
            if (StreamDisableSelector.SubTitlesSelf == selector)
                return true;
            if (StreamDisableSelector.VideoTextSelf == selector)
                return true;

            // No, other
            return false;
        }

        /// <summary>
        /// Meldet, ob eine Deaktivierungsoption sich auf eine Auswahl höherer Priorität
        /// bezieht.
        /// </summary>
        /// <param name="selector">Die Option.</param>
        /// <returns>Gesetzt, wenn die Prüfung erfolgreich war.</returns>
        public static bool IsHigher( this StreamDisableSelector selector )
        {
            // Check it
            if (StreamDisableSelector.AC3IfMP2ExistsHigher == selector)
                return true;
            if (StreamDisableSelector.AlternateAC3Higher == selector)
                return true;
            if (StreamDisableSelector.AlternateMP2Higher == selector)
                return true;
            if (StreamDisableSelector.MP2IfAC3ExistsHigher == selector)
                return true;
            if (StreamDisableSelector.SubTitlesHigher == selector)
                return true;
            if (StreamDisableSelector.VideoTextHigher == selector)
                return true;

            // No, other
            return false;
        }

        /// <summary>
        /// Meldet, ob eine Deaktivierungsoption sich auf eine Auswahl niedrigerer
        /// Priorität bezieht.
        /// </summary>
        /// <param name="selector">Die Option.</param>
        /// <returns>Gesetzt, wenn die Prüfung erfolgreich war.</returns>
        public static bool IsLower( this StreamDisableSelector selector )
        {
            // Check it
            if (StreamDisableSelector.AC3IfMP2ExistsLower == selector)
                return true;
            if (StreamDisableSelector.AlternateAC3Lower == selector)
                return true;
            if (StreamDisableSelector.AlternateMP2Lower == selector)
                return true;
            if (StreamDisableSelector.MP2IfAC3ExistsLower == selector)
                return true;
            if (StreamDisableSelector.SubTitlesLower == selector)
                return true;
            if (StreamDisableSelector.VideoTextLower == selector)
                return true;

            // No, other
            return false;
        }

        /// <summary>
        /// Entfernt den Videotext Teildatenstrom (PID) aus einer Anforderung.
        /// </summary>
        /// <param name="streams">Die gewünschte Anforderung.</param>
        /// <param name="needed">Die Anzahl der benötigten Verbraucher, die geeignet
        /// korrigiert wird.</param>
        /// <param name="report">Protokolliert die vorgenommene Veränderung.</param>
        /// <returns>Gesetzt, wenn eine Änderung vorgenommen wurde.</returns>
        private static bool RemoveVideoText( this StreamSelection streams, ref int needed, Action<string, int> report )
        {
            // Not necessary
            if (needed < 1)
                return false;

            // Not possible
            if (!streams.Videotext)
                return false;

            // Report
            if (null != report)
                report( "VideoText", 1 );

            // Remove
            streams.Videotext = false;

            // Correct counter
            --needed;

            // Report
            return true;
        }

        /// <summary>
        /// Entfernt die Untertitel Teildatenströme (PID) aus einer Anforderung.
        /// </summary>
        /// <param name="streams">Die gewünschte Anforderung.</param>
        /// <param name="needed">Die Anzahl der benötigten Verbraucher, die geeignet
        /// korrigiert wird.</param>
        /// <param name="report">Protokolliert die vorgenommene Veränderung.</param>
        /// <returns>Gesetzt, wenn eine Änderung vorgenommen wurde.</returns>
        private static bool RemoveSubTitles( this StreamSelection streams, ref int needed, Action<string, int> report )
        {
            // Not necessary
            if (needed < 1)
                return false;

            // Get the count
            int count = streams.SubTitles.GetLanguageCount();

            // Not possible
            if (count < 1)
                return false;

            // Report
            if (null != report)
                report( "SubTitles", count );

            // Correct
            needed = Math.Max( 0, needed - count );

            // Wipe out
            streams.SubTitles.LanguageMode = LanguageModes.Selection;
            streams.SubTitles.Languages.Clear();

            // Report
            return true;
        }

        /// <summary>
        /// Entfernt eine alternative MP2 Tonspuren aus einer Anforderung.
        /// </summary>
        /// <param name="streams">Die gewünschte Anforderung.</param>
        /// <param name="needed">Die Anzahl der benötigten Verbraucher, die geeignet
        /// korrigiert wird.</param>
        /// <param name="report">Protokolliert die vorgenommene Veränderung.</param>
        /// <returns>Gesetzt, wenn eine Änderung vorgenommen wurde.</returns>
        private static bool RemoveAlternateMP2Audio( this StreamSelection streams, ref int needed, Action<string, int> report )
        {
            // Use helper
            return streams.MP2Tracks.RemoveAlternateLanguages( ref needed, l => { if (null != report) report( "MP2 " + l, 1 ); } );
        }

        /// <summary>
        /// Entfernt eine alternative AC3 Tonspuren aus einer Anforderung.
        /// </summary>
        /// <param name="streams">Die gewünschte Anforderung.</param>
        /// <param name="needed">Die Anzahl der benötigten Verbraucher, die geeignet
        /// korrigiert wird.</param>
        /// <param name="report">Protokolliert die vorgenommene Veränderung.</param>
        /// <returns>Gesetzt, wenn eine Änderung vorgenommen wurde.</returns>
        private static bool RemoveAlternateAC3Audio( this StreamSelection streams, ref int needed, Action<string, int> report )
        {
            // Use helper
            return streams.AC3Tracks.RemoveAlternateLanguages( ref needed, l => { if (null != report) report( "AC3 " + l, 1 ); } );
        }

        /// <summary>
        /// Meldet die Anzahl der Datenströme für die einzelnen Sprachvarianten.
        /// </summary>
        /// <param name="selection">Beschreibung der Tonspuren.</param>
        /// <returns>Die gewünschte Anzahl.</returns>
        private static int GetLanguageCount( this LanguageSelection selection )
        {
            // Report
            if (selection.LanguageMode == LanguageModes.Primary)
                return 1;
            else
                return selection.Languages.Count;
        }

        /// <summary>
        /// Entfernt eine alle AC3 Tonspuren aus einer Anforderung, wenn mindestens eine MP2
        /// Tonspur vorhanden ist.
        /// </summary>
        /// <param name="streams">Die gewünschte Anforderung.</param>
        /// <param name="needed">Die Anzahl der benötigten Verbraucher, die geeignet
        /// korrigiert wird.</param>
        /// <param name="report">Protokolliert die vorgenommene Veränderung.</param>
        /// <returns>Gesetzt, wenn eine Änderung vorgenommen wurde.</returns>
        private static bool RemoveAC3AudioIfMP2Present( this StreamSelection streams, ref int needed, Action<string, int> report )
        {
            // Not need
            if (needed < 1)
                return false;

            // Load the count
            int count = streams.AC3Tracks.GetLanguageCount();

            // Not possible
            if (count < 1)
                return false;

            // Not possible
            if (streams.MP2Tracks.GetLanguageCount() < 1)
                return false;

            // Report
            if (null != report)
                report( "AC3", count );

            // Correct
            needed = Math.Max( 0, needed - count );

            // Wipe out
            streams.AC3Tracks.LanguageMode = LanguageModes.Selection;
            streams.AC3Tracks.Languages.Clear();

            // Report
            return true;
        }

        /// <summary>
        /// Entfernt eine alle MP2 Tonspuren aus einer Anforderung, wenn mindestens eine AC3
        /// Tonspur vorhanden ist.
        /// </summary>
        /// <param name="streams">Die gewünschte Anforderung.</param>
        /// <param name="needed">Die Anzahl der benötigten Verbraucher, die geeignet
        /// korrigiert wird.</param>
        /// <param name="report">Protokolliert die vorgenommene Veränderung.</param>
        /// <returns>Gesetzt, wenn eine Änderung vorgenommen wurde.</returns>
        private static bool RemoveMP2AudioIfAC3Present( this StreamSelection streams, ref int needed, Action<string, int> report )
        {
            // Not need
            if (needed < 1)
                return false;

            // Get the count
            int count = streams.MP2Tracks.GetLanguageCount();

            // Not possible
            if (count < 1)
                return false;

            // Not possible
            if (streams.AC3Tracks.GetLanguageCount() < 1)
                return false;

            // Report
            if (null != report)
                report( "MP2", count );

            // Correct
            needed = Math.Max( 0, needed - count );

            // Wipe out
            streams.MP2Tracks.LanguageMode = LanguageModes.Selection;
            streams.MP2Tracks.Languages.Clear();

            // Report
            return true;
        }

        /// <summary>
        /// Entfernt eine alternative Tonspure aus einer Anforderung.
        /// </summary>
        /// <param name="languages">Die Liste der angeforderten Tonspuren.</param>
        /// <param name="needed">Die Anzahl der benötigten Verbraucher, die geeignet
        /// korrigiert wird.</param>
        /// <param name="report">Protokolliert die vorgenommene Veränderung.</param>
        /// <returns>Gesetzt, wenn eine Änderung vorgenommen wurde.</returns>
        private static bool RemoveAlternateLanguages( this LanguageSelection languages, ref int needed, Action<string> report )
        {
            // Must be at least two
            if (languages.GetLanguageCount() < 2)
                return false;

            // Get the primary
            string locked = languages.Languages[0];

            // Set flag
            bool removed = false;

            // Skip
            if (string.IsNullOrEmpty( locked ))
                return false;

            // Find the first not equal to the primary
            for (int i = languages.Languages.Count; (needed > 0) && (i-- > 1); )
            {
                // Get the item
                string language = languages.Languages[i];

                // Skip
                if (string.IsNullOrEmpty( language ))
                    continue;

                // Must keep it
                if (0 == string.Compare( language, locked, true ))
                    continue;

                // Remove all these
                for (int j = ++i; j-- > 1; )
                {
                    // Load for test
                    string test = languages.Languages[j];

                    // Not possible
                    if (string.IsNullOrEmpty( test ))
                        continue;

                    // No match
                    if (0 != string.Compare( test, language, true ))
                        continue;

                    // Report
                    report( test );

                    // Remove
                    languages.Languages.RemoveAt( j );

                    // No longer all
                    languages.LanguageMode = LanguageModes.Selection;

                    // Correct
                    if (needed > 0)
                        --needed;

                    // Remember
                    removed = true;

                    // Do move one back
                    --i;
                }
            }

            // None found
            return removed;
        }

        /// <summary>
        /// Wendet eine Deaktivierung auf eine Auswahl von Teildatenströmen (PID) an.
        /// </summary>
        /// <param name="streams">Die aktuelle Auswahl.</param>
        /// <param name="disable">Der gewünschte Aspekt.</param>
        /// <param name="mode">Der aktuelle Korrekturkontext der zugehörigen Quelle.</param>
        /// <param name="needed">Die Anzahl der noch benötigten Verbraucher, die geeignet korrigiert wird.</param>
        /// <param name="report">Protokolliert die vorgenommene Veränderung.</param>
        /// <returns>Gesetzt, wenn eine Veränderung stattgefunden hat.</returns>
        public static bool ApplyDisable( this StreamSelection streams, StreamDisableSelector disable, StreamDisableMode mode, ref int needed, Action<string, int> report )
        {
            // Check mode first
            switch (mode)
            {
                case StreamDisableMode.Self:
                    {
                        // All supported
                        switch (disable)
                        {
                            case StreamDisableSelector.AC3IfMP2ExistsSelf: return streams.RemoveAC3AudioIfMP2Present( ref needed, report );
                            case StreamDisableSelector.AlternateAC3Self: return streams.RemoveAlternateAC3Audio( ref needed, report );
                            case StreamDisableSelector.AlternateMP2Self: return streams.RemoveAlternateMP2Audio( ref needed, report );
                            case StreamDisableSelector.MP2IfAC3ExistsSelf: return streams.RemoveMP2AudioIfAC3Present( ref needed, report );
                            case StreamDisableSelector.SubTitlesSelf: return streams.RemoveSubTitles( ref needed, report );
                            case StreamDisableSelector.VideoTextSelf: return streams.RemoveVideoText( ref needed, report );
                        }

                        // Done
                        break;
                    }
                case StreamDisableMode.Higher:
                    {
                        // All supported
                        switch (disable)
                        {
                            case StreamDisableSelector.AC3IfMP2ExistsHigher: return streams.RemoveAC3AudioIfMP2Present( ref needed, report );
                            case StreamDisableSelector.AlternateAC3Higher: return streams.RemoveAlternateAC3Audio( ref needed, report );
                            case StreamDisableSelector.AlternateMP2Higher: return streams.RemoveAlternateMP2Audio( ref needed, report );
                            case StreamDisableSelector.MP2IfAC3ExistsHigher: return streams.RemoveMP2AudioIfAC3Present( ref needed, report );
                            case StreamDisableSelector.SubTitlesHigher: return streams.RemoveSubTitles( ref needed, report );
                            case StreamDisableSelector.VideoTextHigher: return streams.RemoveVideoText( ref needed, report );
                        }

                        // Done
                        break;
                    }
                case StreamDisableMode.Lower:
                    {
                        // All supported
                        switch (disable)
                        {
                            case StreamDisableSelector.AC3IfMP2ExistsLower: return streams.RemoveAC3AudioIfMP2Present( ref needed, report );
                            case StreamDisableSelector.AlternateAC3Lower: return streams.RemoveAlternateAC3Audio( ref needed, report );
                            case StreamDisableSelector.AlternateMP2Lower: return streams.RemoveAlternateMP2Audio( ref needed, report );
                            case StreamDisableSelector.MP2IfAC3ExistsLower: return streams.RemoveMP2AudioIfAC3Present( ref needed, report );
                            case StreamDisableSelector.SubTitlesLower: return streams.RemoveSubTitles( ref needed, report );
                            case StreamDisableSelector.VideoTextLower: return streams.RemoveVideoText( ref needed, report );
                        }

                        // Done
                        break;
                    }
            }

            // None found
            return false;
        }
    }
}
