namespace VCRNETClient.App.NoUi {

    // Bietet die Daten eines Auftrags zur Pflege an.
    export class JobEditor {
        constructor(private _job: VCRServer.EditJobContract, devices: ISelectableValue<string>[], favoriteSources: string[], folders: ISelectableValue<string>[], onChange: () => void) {
            // Pflegekomponenten erstellen
            this.name = new StringEditor(_job, "name", onChange, true);
            this.deviceLock = new BooleanEditor(_job, "lockedToDevice", onChange);
            this.allLanguages = new BooleanEditor(_job, "allLanguages", onChange);
            this.includeDolby = new BooleanEditor(_job, "includeDolby", onChange);
            this.withVideotext = new BooleanEditor(_job, "withVideotext", onChange);
            this.withSubtitles = new BooleanEditor(_job, "withSubtitles", onChange);
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

        // Gesetzt um alle Sprachen aufzuzeichnen
        readonly allLanguages: BooleanEditor;

        // Gesetzt, um die Dolby Digital Tonspur aufzuzeichnen
        readonly includeDolby: BooleanEditor;

        // Gesetzt, um den Videotext aufzuzeichnen
        readonly withVideotext: BooleanEditor;

        // Gesetzt, um die Untertitel aufzuzeichnen
        readonly withSubtitles: BooleanEditor;

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
            this.allLanguages.validate();
            this.includeDolby.validate();
            this.withVideotext.validate();
            this.withSubtitles.validate();
        }
    }

}