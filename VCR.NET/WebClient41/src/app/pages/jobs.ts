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

    export class JobPage extends Page implements IJobPage {

        private static readonly _types = [
            JMSLib.App.uiValue(false, "Aktiv"),
            JMSLib.App.uiValue(true, "Archiviert"),
        ];

        readonly showArchived = new JMSLib.App.SelectSingleFromList({}, "value", null, () => this.refreshUi(), JobPage._types);

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

                this.application.isBusy = false;
            });
        }

        private createSchedule(schedule: VCRServer.InfoScheduleContract): IJobPageSchedule {
            var name = schedule.name || `Aufzeichnung`;
            var repeat = schedule.repeatPattern;
            var start: string = ``;

            if (repeat === 0)
                start = JMSLib.App.DateTimeUtils.formatStartTime(new Date(schedule.start));
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

                start += ` ${JMSLib.App.DateTimeUtils.formatEndTime(new Date(schedule.start))}`;
            }

            return { name: `${name}: ${start} auf ${schedule.sourceName}`, url: `${this.application.editPage.route};id=${schedule.id}` };
        }

        get title(): string {
            return `Alle Aufträge`;
        }
    }
}