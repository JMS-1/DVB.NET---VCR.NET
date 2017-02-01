/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IAdminPage extends IPage {
    }

    export class AdminPage extends Page<JMSLib.App.ISite> implements IAdminPage {

        constructor(application: Application) {
            super("admin", application);
        }

        reset(sections: string[]): void {
            window.setTimeout(() => this.application.setBusy(false), 0);
        }

        get title(): string {
            return `Administration und Konfiguration`;
        }
    }
}