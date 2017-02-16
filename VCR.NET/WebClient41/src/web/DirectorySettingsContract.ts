module VCRServer {

    // Repräsentiert die Klasse DirectorySettings
    export interface DirectorySettingsContract extends SettingsContract {
        // Alle Aufzeichnungsverzeichnisse
        directories: string[];

        // Das Muster für die Erzeugung von Dateinamen
        pattern: string;
    }

}

