module VCRServer {

    // Repräsentiert die Klasse ProfileSettings
    export interface ProfileSettingsContract extends SettingsContract {
        // Alle DVB.NET Geräte auf dem Rechner, auf dem der VCR.NET Recording Service läuft
        profiles: ProfileContract[];

        // Das bevorzugte Gerät für neue Aufzeichnungen
        defaultProfile: string;
    }

    export function getProfileSettings(): Promise<ProfileSettingsContract> {
        return doUrlCall(`configuration?devices`);
    }

    export function setProfileSettings(data: ProfileSettingsContract): Promise<boolean> {
        return doUrlCall(`configuration?devices`, `PUT`, data);
    }

}

