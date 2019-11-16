module VCRServer {

    // Repräsentiert die Klasse UserProfile
    export interface UserProfileContract {
        // Die Anzahl der Einträge im Aufzeichnungsplan
        planDays: number;

        // Die Liste der zuletzt verwendeten Quellen
        recentSources: string[];

        // Die Art der Quelle
        typeFilter: string;

        // Die Art der Verschlüsselung
        encryptionFilter: string;

        // Gesetzt, wenn alle Sprachen aufgezeichnet werden sollen
        languages: boolean;

        // Gesetzt, wenn die Dolby Digital Tonspur aufgezeichnet werden soll
        dolby: boolean;

        // Gesetzt, wenn der Videotext aufgezeichnet werden soll
        videotext: boolean;

        // Gesetzt, wenn DVB Untertitel aufgezeichnet werden sollen
        subtitles: boolean;

        // Gesetzt, wenn bei der Programmierung aus der Programmzeitschrift heraus danach in diese zurück gekehrt werden soll
        backToGuide: boolean;

        // Die Anzahl der Sendungen auf einer Seite der Programmzeitschrift
        guideRows: number;

        // Die Vorlaufzeit bei Programmierung aus der Programmzeitschrift heraus
        guideAheadStart: number;

        // Die Nachlaufzeit bei Programmierung aus der Programmzeitschrift heraus
        guideBeyondEnd: number;

        // Die maximal erlaubte Anzahl von zuletzt verwendeten Quellen
        recentSourceLimit: number;

        // Gesetzt, wenn beim Abbruch einer Aufzeichnung der Übergang in den Schlafzustand deaktiviert werden soll
        suppressHibernate: boolean;

        // Die gespeicherten Suchen der Programmzeitschrift
        guideSearches: string;
    }

    export function getUserProfile(): Promise<UserProfileContract> {
        return doUrlCall(`userprofile`);
    }

    export function setUserProfile(profile: UserProfileContract): Promise<UserProfileContract> {
        return doUrlCall(`userprofile`, `PUT`, profile);
    }

}

