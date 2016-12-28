/// <reference path="jobSchedule.ts" />

namespace VCRNETClient.App.NoUi {

    // Schnittstelle zur Pflege eines Auftrags.
    export interface IJobEditor extends IJobScheduleEditor {
        // Das Aufzeichnungsverzeichnis.
        readonly folder: IStringFromListEditor;

        // Das zu verwendende DVB Gerät.
        readonly device: IStringFromListEditor;

        // Gesetzt, wenn die Aufzeichnung immer auf dem Gerät stattfinden soll.
        readonly deviceLock: IBooleanEditor;
    }

    // Bietet die Daten eines Auftrags zur Pflege an.
    export class JobEditor extends JobScheduleEditor<VCRServer.EditJobContract> implements IJobEditor {
        constructor(model: VCRServer.EditJobContract, devices: ISelectableValue<string>[], favoriteSources: string[], folders: ISelectableValue<string>[], onChange: () => void) {
            super(model, true, favoriteSources, onChange);

            // Pflegekomponenten erstellen
            this.deviceLock = new BooleanEditor(this.model, "lockedToDevice", onChange);
            this.device = new StringListEditor(this.model, "device", onChange, true, devices);
            this.folder = new StringListEditor(this.model, "directory", onChange, false, folders);
        }

        // Das Aufzeichnungsverzeichnis.
        readonly folder: StringListEditor;

        // Das zu verwendende DVB Gerät.
        readonly device: StringListEditor;

        // Gesetzt, wenn die Aufzeichnung immer auf dem Gerät stattfinden soll.
        readonly deviceLock: BooleanEditor;

        // Prüft alle Daten.
        validate(sources: VCRServer.SourceEntry[]): void {
            super.validate(sources, false);

            // Lokalisierte Prüfungen.
            this.device.validate();
            this.folder.validate();
            this.deviceLock.validate();
        }
    }

}