/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IJobPage extends IPage {
        readonly showArchived: JMSLib.App.IValueFromList<boolean>;
    }

    export class JobPage extends Page<JMSLib.App.ISite> implements IJobPage {

        private static readonly _types: JMSLib.App.IUiValue<boolean>[] = [
            { display: "Aktiv", value: false },
            { display: "Archiviert", value: true },
        ];

        readonly showArchived = new JMSLib.App.EditFromList<boolean>({}, "value", () => this.refreshUi(), null, JobPage._types);

        constructor(application: Application) {
            super("jobs", application);
        }

        reset(sections: string[]): void {
            this.showArchived.value = (sections[0] === "archive");

            VCRServer.getInfoJobs().then(info => {
                this.application.setBusy(false);
            });
        }

        get title(): string {
            return `Alle Aufträge`;
        }
    }
}