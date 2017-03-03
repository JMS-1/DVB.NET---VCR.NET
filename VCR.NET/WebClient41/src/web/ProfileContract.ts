module VCRServer {

    // Repräsentiert die Klasse ConfigurationProfile
    export interface ProfileContract {
        // Der Name des Gerätes
        name: string;

        // Gesetzt, wenn es für Aufzeichnungen verwendet werden darf
        active: boolean;

        // Die maximale Anzahl gleichzeitig empfangener Quellen
        sourceLimit: number;

        // Die maximale Anzahl gleichzeitig entschlüsselbarer Quellen
        ciLimit: number;

        // Die Aufzeichnungspriorität
        priority: number;
    }

}

