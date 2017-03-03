module VCRServer {

    // Repräsentiert die Klasse InfoSchedule
    export interface InfoScheduleContract {
        // Der Name der Aufzeichnung
        name: string;

        // Der erste Start der Aufzeichnung in ISO Notation
        start: string;

        // Die Wochentage, an denen die Aufzeichnugn wiederholt werden soll
        repeatPattern: number;

        // Der Name der Quelle
        sourceName: string;

        // Die eindeutige Kennung der Aufzeichnung
        id: string;
    }

}

