module VCRServer {

    // Repräsentiert die Klasse InfoService
    export interface InfoServiceContract {
        // Die aktuelle Version des Dienstes in der Notation MAJOR.MINOR [DD.MM.YYYY]
        version: string;

        // Die aktuelle Version der Installation in der Notation MAJOR.MINOR.BUILD
        msiVersion: string;

        // Gesetzt, wenn mindestens ein Gerät eine Aufzeichnung oder Aufgabe ausführt
        active: boolean;

        // Gesetzt, wenn der Anwender ein Administrator ist
        userIsAdmin: boolean;

        // Gesetzt, wenn die Aktualisierung der Quellen verfügbar ist
        canScan: boolean;

        // Gesetzt, wenn die Aktualisierung der Programmzeitschrift verfügbar ist
        hasGuides: boolean;
    }

    export function getServerVersion(): Promise<InfoServiceContract> {
        return doUrlCall(`info`);
    }

}

