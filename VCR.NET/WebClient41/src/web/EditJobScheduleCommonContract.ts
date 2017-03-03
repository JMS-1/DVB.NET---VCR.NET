module VCRServer {

    // Gemeinsame Schnittstelle der Klassen EditSchedule und EditJob
    export interface EditJobScheduleCommonContract {
        // Der Name der Aufzeichnung
        name: string;

        // Die verwendete Quelle
        sourceName: string;

        // Gesetzt, wenn alle Sprachen aufgezeichnet werden sollen
        allLanguages: boolean;

        // Gesetzt, wenn die Dolby Digital Tonspur aufgezeichnet werden soll
        includeDolby: boolean;

        // Gesetzt, wenn der Videotext aufgezeichnet werden soll
        withVideotext: boolean;

        // Gesetzt, wenn Untertitel aufgezeichnet werden sollen
        withSubtitles: boolean;
    }

}

