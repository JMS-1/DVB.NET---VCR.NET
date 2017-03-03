module VCRServer {

    // Repräsentiert die Klasse DirectorySettings
    export interface DirectorySettingsContract extends SettingsContract {
        // Alle Aufzeichnungsverzeichnisse
        directories: string[];

        // Das Muster für die Erzeugung von Dateinamen
        pattern: string;
    }

    export function getDirectorySettings(): JMSLib.App.IHttpPromise<DirectorySettingsContract> {
        return doUrlCall(`configuration?directory`);
    }

    export function setDirectorySettings(data: DirectorySettingsContract): JMSLib.App.IHttpPromise<boolean> {
        return doUrlCall(`configuration?directory`, `PUT`, data);
    }

    export function validateDirectory(path: string): JMSLib.App.IHttpPromise<boolean> {
        return doUrlCall(`configuration?validate&directory=${encodeURIComponent(path)}`);
    }

    export function browseDirectories(root: string, children: boolean): JMSLib.App.IHttpPromise<string[]> {
        return doUrlCall(`configuration?browse&toParent=${!children}&root=${encodeURIComponent(root)}`);
    }

}

