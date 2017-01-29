/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IJobPage extends IPage {
    }

    export class JobPage extends Page<JMSLib.App.ISite> implements IJobPage {
        constructor(application: Application) {
            super("jobs", application);
        }

        reset(sections: string[]): void {
            setTimeout(() => this.application.setBusy(false), 0);
        }

        get title(): string {
            return `Alle Aufträge`;
        }
    }
}