/// <reference path="jobSchedule.ts" />

namespace VCRNETClient.App.Edit {

    // Schnittstelle zur Pflege eines Auftrags.
    export interface IJobEditor extends IJobScheduleEditor {
        // Das Aufzeichnungsverzeichnis.
        readonly folder: JMSLib.App.IValueFromList<string>;

        // Das zu verwendende DVB Gerät.
        readonly device: JMSLib.App.IValueFromList<string>;

        // Gesetzt, wenn die Aufzeichnung immer auf dem Gerät stattfinden soll.
        readonly deviceLock: JMSLib.App.IEditFlag;
    }

    // Bietet die Daten eines Auftrags zur Pflege an.
    export class JobEditor extends JobScheduleEditor<VCRServer.EditJobContract> implements IJobEditor {
        constructor(page: IPage, model: VCRServer.EditJobContract, devices: JMSLib.App.IUiValue<string>[], favoriteSources: string[], folders: JMSLib.App.IUiValue<string>[], onChange: () => void) {
            super(page, model, true, favoriteSources, onChange);

            // Pflegekomponenten erstellen
            this.deviceLock = new JMSLib.App.EditFlag(this.model, "lockedToDevice", "(auf diesem Gerät aufzeichnen)", onChange);
            this.device = new JMSLib.App.SelectSingleFromList<string>(this.model, "device", "DVB.NET Geräteprofil", onChange, true, devices);
            this.folder = new JMSLib.App.SelectSingleFromList<string>(this.model, "directory", "Verzeichnis", onChange, false, folders);
        }

        // Das Aufzeichnungsverzeichnis.
        readonly folder: JMSLib.App.SelectSingleFromList<string>;

        // Das zu verwendende DVB Gerät.
        readonly device: JMSLib.App.SelectSingleFromList<string>;

        // Gesetzt, wenn die Aufzeichnung immer auf dem Gerät stattfinden soll.
        readonly deviceLock: JMSLib.App.EditFlag;

        // Prüft alle Daten.
        validate(sources: VCRServer.SourceEntry[]): void {
            super.validate(sources, false);

            // Lokalisierte Prüfungen.
            this.device.validate();
            this.folder.validate();
            this.deviceLock.validate();
        }

        // Gesetzt, wenn die Einstellungen des Auftrags gültig sind.
        isValid(): boolean {
            if (!super.isValid())
                return false;
            if (this.device.message.length > 0)
                return false;
            if (this.folder.message.length > 0)
                return false;
            if (this.deviceLock.message.length > 0)
                return false;

            return true;
        }
    }

}