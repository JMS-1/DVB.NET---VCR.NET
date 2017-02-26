/// <reference path="jobSchedule.ts" />

namespace VCRNETClient.App.Edit {

    // Schnittstelle zur Pflege eines Auftrags.
    export interface IJobEditor extends IJobScheduleEditor {
        // Das Aufzeichnungsverzeichnis.
        readonly folder: JMSLib.App.IValueFromList<string>;

        // Das zu verwendende DVB Gerät.
        readonly device: JMSLib.App.IValueFromList<string>;

        // Gesetzt, wenn die Aufzeichnung immer auf dem Gerät stattfinden soll.
        readonly deviceLock: JMSLib.App.IFlag;
    }

    // Bietet die Daten eines Auftrags zur Pflege an.
    export class JobEditor extends JobScheduleEditor<VCRServer.EditJobContract> implements IJobEditor {

        // Erstellt ein neues Präsentationsmodell.
        constructor(page: IPage, model: VCRServer.EditJobContract, devices: JMSLib.App.IUiValue<string>[], favoriteSources: string[], folders: JMSLib.App.IUiValue<string>[], onChange: () => void) {
            super(page, model, favoriteSources, onChange);

            // Pflegekomponenten erstellen
            this.deviceLock = new JMSLib.App.Flag(this.model, "lockedToDevice", "(auf diesem Gerät aufzeichnen)", onChange);
            this.folder = new JMSLib.App.SelectSingleFromList(this.model, "directory", "Verzeichnis", onChange, folders);
            this.device = new JMSLib.App.SelectSingleFromList(this.model, "device", "DVB.NET Geräteprofil", onChange, devices)
                .addRequiredValidator();

            // Zusätzliche Prüfungen einrichten.
            this.name.addRequiredValidator(`Ein Auftrag muss einen Namen haben.`);

            // Initiale Prüfung.
            this.name.validate();
            this.device.validate();
            this.folder.validate();
            this.deviceLock.validate();
        }

        // Das Aufzeichnungsverzeichnis.
        readonly folder: JMSLib.App.SelectSingleFromList<string>;

        // Das zu verwendende DVB Gerät.
        readonly device: JMSLib.App.SelectSingleFromList<string>;

        // Gesetzt, wenn die Aufzeichnung immer auf dem Gerät stattfinden soll.
        readonly deviceLock: JMSLib.App.Flag;

        // Gesetzt, wenn die Einstellungen des Auftrags gültig sind.
        isValid(): boolean {
            // Erst einmal die Basisklasse fragen.
            if (!super.isValid())
                return false;

            // Dann alle unsere eigenen Präsentationsmodelle.
            if (this.device.message)
                return false;
            if (this.folder.message)
                return false;
            if (this.deviceLock.message)
                return false;

            return true;
        }
    }

}