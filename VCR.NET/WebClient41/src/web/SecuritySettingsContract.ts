module VCRServer {

    // Repräsentiert die Klasse SecuritySettings
    export interface SecuritySettingsContract extends SettingsContract {
        // Die Windows Gruppe der normalen Benutzer
        users: string;

        // Die Windows Gruppe der Administratoren
        admins: string;
    }

}

