namespace VCRNETClient.App.NoUi {

    // Bietet die Daten eines Auftrags zur Pflege an.
    export class JobEditor {
        constructor(private _job: VCRServer.EditJobContract, devices: ISelectableValue<string>[], favoriteSources: string[], folders: ISelectableValue<string>[], onChange: () => void) {
            // Pflegekomponenten erstellen
            this.name = new StringEditor(_job, "name", onChange, true);
            this.deviceLock = new BooleanEditor(_job, "lockedToDevice", onChange);
            this.device = new StringListEditor(_job, "device", onChange, true, devices);
            this.source = new ChannelEditor(_job, "sourceName", favoriteSources, onChange);
            this.folder = new StringListEditor(_job, "directory", onChange, false, folders);
        }

        // Der Name des Auftrags.
        readonly name: StringEditor;

        // Das Aufzeichnungsverzeichnis.
        readonly folder: StringListEditor;

        // Das zu verwendende DVB Gerät.
        readonly device: StringListEditor;

        // Gesetzt, wenn die Aufzeichnung immer auf dem Gerät stattfinden soll.
        readonly deviceLock: BooleanEditor;

        // Der Name der Quelle, die aufgezeichnet werden soll.
        readonly source: ChannelEditor;

        // Prüft alle Daten.
        validate(sources: VCRServer.SourceEntry[]): void {
            // Aktualisieren.
            this.source.setSources(sources);

            // Lokalisierte Prüfungen.
            this.name.validate();
            this.device.validate();
            this.folder.validate();
            this.source.validate();
            this.deviceLock.validate();
        }
    }

}