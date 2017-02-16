module VCRServer {

    // Repräsentiert die Klasse EditJob
    export interface EditJobContract extends EditJobScheduleCommonContract {
        // Das zu verwendende Aufzeichnungsverzeichnis
        directory: string;

        // Das zu verwendende Gerät
        device: string;

        // Gesetzt, wenn die Aufzeichnung auf jeden Fall auf dem angegebenen Geräte erfolgen soll
        lockedToDevice: boolean;
    }

}

