/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface ISettingsPage extends IPage {
    }

    export class SettingsPage extends Page implements ISettingsPage {

        constructor(application: Application) {
            super(`settings`, application);
        }

        reset(sections: string[]): void {
            this.application.isBusy = false;
        }

        get title(): string {
            return `Individuelle Einstellungen ändern`;
        }
    }
}