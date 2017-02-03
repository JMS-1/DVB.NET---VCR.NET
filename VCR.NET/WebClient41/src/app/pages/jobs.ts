/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IJobPageSchedule {
        readonly name: string;

        readonly url: string;
    }

    export interface IJobPageJob {
        readonly name: string;

        readonly schedules: IJobPageSchedule[];
    }

    interface IJobPageJobInternal extends IJobPageJob {
        readonly isActive: boolean;
    }

    export interface IJobPage extends IPage {
        readonly showArchived: JMSLib.App.IValueFromList<boolean>;

        readonly jobs: IJobPageJob[];
    }

    export class JobPage extends Page<JMSLib.App.ISite> implements IJobPage {

        private static readonly _types: JMSLib.App.IUiValue<boolean>[] = [
            { display: "Aktiv", value: false },
            { display: "Archiviert", value: true },
        ];

        readonly showArchived = new JMSLib.App.EditFromList<boolean>({}, "value", () => this.refreshUi(), null, JobPage._types);

        private _jobs: IJobPageJobInternal[] = [];

        get jobs(): IJobPageJob[] {
            const archived = this.showArchived.value;

            return this._jobs.filter(job => job.isActive !== archived);
        }

        constructor(application: Application) {
            super("jobs", application);
        }

        reset(sections: string[]): void {
            this.showArchived.value = (sections[0] === "archive");

            VCRServer.getInfoJobs().then(info => {
                this._jobs = info.map(j => {
                    var job: IJobPageJobInternal = { name: j.name, isActive: j.active, schedules: [] };

                    job.schedules.push({ name: "(Neue Aufzeichnung)", url: `${this.application.editPage.route};id=${j.id}` });
                    job.schedules.push(...j.schedules.map(s => this.createSchedule(s)));

                    return job;
                });

                this.application.setBusy(false);
            });
        }

        private createSchedule(schedule: VCRServer.InfoScheduleContract): IJobPageSchedule {
            var name = schedule.name || `Aufzeichnung`;
            var repeat = schedule.repeatPattern;
            var start: string = ``;

            if (repeat === 0)
                start = JMSLib.DateFormatter.getStartTime(new Date(schedule.start));
            else {
                if (repeat & Edit.ScheduleEditor.flagMonday)
                    start += JMSLib.DateFormatter.germanDays[1];
                if (repeat & Edit.ScheduleEditor.flagTuesday)
                    start += JMSLib.DateFormatter.germanDays[2]
                if (repeat & Edit.ScheduleEditor.flagWednesday)
                    start += JMSLib.DateFormatter.germanDays[3];
                if (repeat & Edit.ScheduleEditor.flagThursday)
                    start += JMSLib.DateFormatter.germanDays[4];
                if (repeat & Edit.ScheduleEditor.flagFriday)
                    start += JMSLib.DateFormatter.germanDays[5];
                if (repeat & Edit.ScheduleEditor.flagSaturday)
                    start += JMSLib.DateFormatter.germanDays[6];
                if (repeat & Edit.ScheduleEditor.flagSunday)
                    start += JMSLib.DateFormatter.germanDays[0];

                start += ` ${JMSLib.DateFormatter.getEndTime(new Date(schedule.start))}`;
            }

            return { name: `${name}: ${start} auf ${schedule.sourceName}`, url: `${this.application.editPage.route};id=${schedule.id}` };
        }

        get title(): string {
            return `Alle Aufträge`;
        }
    }
}