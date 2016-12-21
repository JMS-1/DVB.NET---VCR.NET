/// <reference path="page.ts" />

namespace VCRNETClient.App {
    export class EditPage extends Page {
        job: JobData;

        schedule: ScheduleData;

        getName(): string {
            return "edit";
        }

        constructor(application: Application) {
            super(application);
        }

        reset(section: string): void {
            VCRServer.createScheduleFromGuide(section.substr(3), "").then(info => {
                this.job = new JobData(info, null);
                this.schedule = new ScheduleData(info);

                this.application.setBusy(false);
            });
        }

        showNew(): boolean {
            return false;
        }

        getTitle(): string {
            return `[TBD]`;
        }
    }
}