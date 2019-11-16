module VCRServer {

    // Repräsentiert die Klasse GuideItem
    export interface GuideItemContract {
        // Der Startzeitpunkt in ISO Notation
        start: string;

        // Die Dauer in Sekunden
        duration: number;

        // Der Name der Sendung
        name: string;

        // Die Sprache, in der die Sendung ausgestrahlt wird
        language: string;

        // Die Quelle
        station: string;

        // Die Liste der Alterfreigaben
        ratings: string[];

        // Die Liste der Kategorien
        categories: string[];

        // Die ausführliche Beschreibung
        description: string;

        // Die Kurzbeschreibung
        shortDescription: string;

        // Gesetzt, wenn das Ende der Sendung in der Zukunft liegt
        active: boolean;

        // Die eindeutige Kennung der Sendung
        id: string;
    }

    export function getGuideItem(device: string, source: string, start: Date, end: Date): Promise<GuideItemContract> {
        return doUrlCall(`guide?profile=${encodeURIComponent(device)}&source=${source}&pattern=${start.getTime()}-${end.getTime()}`);
    }

}

