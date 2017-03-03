module VCRServer {

    // Repräsentiert die Klasse SecuritySettings
    export interface SecuritySettingsContract extends SettingsContract {
        // Die Windows Gruppe der normalen Benutzer
        users: string;

        // Die Windows Gruppe der Administratoren
        admins: string;
    }

    export function getSecuritySettings(): JMSLib.App.IHttpPromise<SecuritySettingsContract> {
        return doUrlCall(`configuration?security`);
    }

    export function setSecuritySettings(data: SecuritySettingsContract): JMSLib.App.IHttpPromise<boolean> {
        return doUrlCall(`configuration?security`, `PUT`, data);
    }

    export function getWindowsGroups(): JMSLib.App.IHttpPromise<string[]> {
        return doUrlCall(`info?groups`);
    }

}

