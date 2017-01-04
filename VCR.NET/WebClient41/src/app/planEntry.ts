namespace VCRNETClient.App {

    // Erweiterte Schnittstelle (View Model) zur Anzeige eines Eintrags des Aufzeichnunsplans.
    export interface IPlanEntry extends VCRServer.PlanActivityContract {
        // Ein eindeutiger Name.
        key: string;

        // Ein Kürzel für die Qualität der Aufzeichnung, etwa ob dieser verspätet beginnt.
        mode: string;

        // Der Zeitpunkt, an dem die Aufzeichnung enden wird.
        end: Date;

        // Der Startzeitpunkt formatiert für die Darstellung.
        displayStart: string;

        // Der Endzeitpunkt, formatiert für die Darstellung - es werden nur Stunden und Minuten angezeigt.
        displayEnd: string;

        // Die zugehörige Ausnahmeregel.
        exception: IPlanException;
    }

    // Initialisiert ein View Model für einen Eintrag des Aufzeichnungsplans.
    export function enrichPlanEntry(entry: VCRServer.PlanActivityContract, key: string): IPlanEntry {
        if (!entry)
            return null;

        // Das Model vom Server wird nur gekapselt und ist unveränderlich - eine JSON Serialisierung ist so allerdings nicht mehr möglich.
        var enriched = <IPlanEntry>{ ["__proto__"]: entry };

        // Defaultwerte einsetzen
        enriched.key = key;

        if (enriched.station == null)
            enriched.station = '(unbekannt)';
        if (enriched.device == null)
            enriched.device = '';

        // Zeiten umrechnen
        var duration = 1000 * <any>enriched.duration;
        var start = new Date(enriched.start);
        var end = new Date(start.getTime() + duration);

        // Daten aus der Rohdarstellung in das Modell kopieren
        enriched.displayStart = DateFormatter.getStartTime(enriched.start = start);
        enriched.displayEnd = DateFormatter.getEndTime(enriched.end = end);
        enriched.duration = duration / 1000;

        // Ausnahmen auswerten
        enriched.exception = enrichPlanException(enriched.exception);

        // Aufzeichungsmodus ermitteln
        if (enriched.station !== 'PSI')
            if (enriched.station !== 'EPG')
                if (enriched.lost)
                    enriched.mode = 'lost';
                else if (enriched.late)
                    enriched.mode = 'late';
                else
                    enriched.mode = 'intime';

        return enriched;
    }
}