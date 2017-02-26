/// <reference path="page.ts" />

namespace VCRNETClient.App {

    // Schnittstelle zur Anzeige einer Aufzeichnung.
    export interface IJobPageSchedule {
        // Der Name der Aufzeichnung.
        readonly name: string;

        // Der Verweis zur Pflege der Aufzeichnung.
        readonly url: string;
    }

    // Schnittstelle zur Anzeige eines Auftrags.
    export interface IJobPageJob {
        // Der Name des Auftrags.
        readonly name: string;

        // Alle Aufzeichnungen zum Auftrag.
        readonly schedules: IJobPageSchedule[];

        // Gesetzt, wenn sich der Auftrag noch nicht im Archiv befindet.
        readonly isActive: boolean;
    }

    // Schnittstelle zur Anzeige der Auftragsübersicht.
    export interface IJobPage extends IPage {
        // Schaltet zwischen der Ansicht der aktiven und archivierten Aufzeichnungen um.
        readonly showArchived: JMSLib.App.IValueFromList<boolean>;

        // Alle Aufträge.
        readonly jobs: IJobPageJob[];
    }

    // Präsentationsmodell zur Ansicht aller Aufträge.
    export class JobPage extends Page implements IJobPage {

        // Optionen zur Auswahl der Art des Auftrags.
        private static readonly _types = [
            JMSLib.App.uiValue(false, "Aktiv"),
            JMSLib.App.uiValue(true, "Archiviert"),
        ];

        // Schaltet zwischen der Ansicht der aktiven und archivierten Aufzeichnungen um.
        readonly showArchived = new JMSLib.App.SelectSingleFromList({}, "value", null, () => this.refreshUi(), JobPage._types);

        // Alle Aufträge.
        private _jobs: IJobPageJob[] = [];

        get jobs(): IJobPageJob[] {
            const archived = this.showArchived.value;

            return this._jobs.filter(job => job.isActive !== archived);
        }

        // Erstellt ein neues Präsentationsmodell.
        constructor(application: Application) {
            super("jobs", application);
        }

        // Initialisiert die Anzeige des Präsentationsmodells.
        reset(sections: string[]): void {
            // Bereich vorwählen.
            this.showArchived.value = (sections[0] === "archive");

            // Aktuelle Liste von VCR.NET Recording Service abrufen.
            VCRServer.getInfoJobs().then(info => {
                // Rohdaten in primitive Präsentationsmodell wandeln.
                this._jobs = info.map(j => {
                    var job: IJobPageJob = { name: j.name, isActive: j.active, schedules: [] };

                    // Es wird immer ein erster Eintrag zum Anlegen neuer Aufzeichnungen zum Auftrag hinzugefügt.
                    job.schedules.push({ name: "(Neue Aufzeichnung)", url: `${this.application.editPage.route};id=${j.id}` });
                    job.schedules.push(...j.schedules.map(s => this.createSchedule(s)));

                    return job;
                });

                // Die Anwendung wird nun zur Bedienung freigegeben.
                this.application.isBusy = false;

                // Oberfläche zur Aktualisierung auffordern.
                this.refreshUi();
            });
        }

        // Erstellt ein neues Präsenationsmodell für eine einzelne Aufzeichnung.
        private createSchedule(schedule: VCRServer.InfoScheduleContract): IJobPageSchedule {
            // Der Name ist je nach konkreter Konfiguration etwas aufwändiger zu ermitteln.
            var name = schedule.name || `Aufzeichnung`;
            var startTime = new Date(schedule.start);
            var repeat = schedule.repeatPattern;
            var start: string = ``;

            if (repeat === 0)
                start = JMSLib.App.DateTimeUtils.formatStartTime(startTime);
            else {
                if (repeat & Edit.ScheduleEditor.flagMonday)
                    start += JMSLib.App.DateTimeUtils.germanDays[1];
                if (repeat & Edit.ScheduleEditor.flagTuesday)
                    start += JMSLib.App.DateTimeUtils.germanDays[2]
                if (repeat & Edit.ScheduleEditor.flagWednesday)
                    start += JMSLib.App.DateTimeUtils.germanDays[3];
                if (repeat & Edit.ScheduleEditor.flagThursday)
                    start += JMSLib.App.DateTimeUtils.germanDays[4];
                if (repeat & Edit.ScheduleEditor.flagFriday)
                    start += JMSLib.App.DateTimeUtils.germanDays[5];
                if (repeat & Edit.ScheduleEditor.flagSaturday)
                    start += JMSLib.App.DateTimeUtils.germanDays[6];
                if (repeat & Edit.ScheduleEditor.flagSunday)
                    start += JMSLib.App.DateTimeUtils.germanDays[0];

                start += ` ${JMSLib.App.DateTimeUtils.formatEndTime(startTime)}`;
            }

            // Präsenationsmodell erstellen.
            return { name: `${name}: ${start} auf ${schedule.sourceName}`, url: `${this.application.editPage.route};id=${schedule.id}` };
        }

        // Der Name des Präsentationsmodell zur Darstellung einer Überschrift in der Oberfläche.
        get title(): string {
            return `Alle Aufträge`;
        }

    }

}