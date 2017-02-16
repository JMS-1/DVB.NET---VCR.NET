module VCRServer {

    // Repräsentiert die Klasse ProfileSettings
    export interface ProfileSettingsContract extends SettingsContract {
        // Alle DVB.NET Geräte auf dem Rechner, auf dem der VCR.NET Recording Service läuft
        profiles: ProfileContract[];

        // Das bevorzugte Gerät für neue Aufzeichnungen
        defaultProfile: string;
    }

}

