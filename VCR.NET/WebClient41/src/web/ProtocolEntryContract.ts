module VCRServer {

    // Repräsentiert die Klasse ProtocolEntry
    export interface ProtocolEntryContract {
        // Der Startzeitpunkt in ISO Notation
        start: string;

        // Der Endzeitpunkt in ISO Notation
        end: string;

        // Der Name der zuerst verwendeten Quelle
        firstSourceName: string;

        // Der Name der primären Aufzeichnungsdatei
        primaryFile: string;

        // Die Liste der erzeugten Dateien
        files: string[];

        // Ein Hinweis zur Anzeige der Größe
        size: string;
    }

}

