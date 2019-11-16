module VCRServer {

    // Repräsentiert die Klasse GuideFilter
    export interface GuideFilterContract {
        // Der Name des aktuell ausgewählten Geräteprofils
        device: string;

        // Der Name der aktuell ausgewählten Quelle
        station: string;

        // Der minimale Startzeitpunkt in ISO Notation
        start: string;

        // Das Suchmuster für den Namen einer Sendung
        title: string;

        // Das Suchmuster für die Beschreibung einer Sendung
        content: string;

        // Die Anzahl von Sendungen pro Anzeigeseite
        size: number;

        // Die aktuelle Seite
        index: number;

        // Einschränkung auf die Art der Quellen
        typeFilter: GuideSource;

        // Einschränkung auf die Verschlüsselung der Quellen
        cryptFilter: GuideEncryption;
    }

    export function queryProgramGuide(filter: GuideFilterContract): Promise<GuideItemContract[]> {
        return doUrlCall(`guide`, `POST`, filter);
    }

    export function countProgramGuide(filter: GuideFilterContract): Promise<number> {
        return doUrlCall(`guide?countOnly`, `POST`, filter);
    }

}

