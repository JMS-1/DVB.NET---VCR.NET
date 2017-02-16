/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IDevicesPage extends IPage {
    }

    export class DevicesPage extends Page implements IDevicesPage {

        constructor(application: Application) {
            super(`current`, application);
        }

        reset(sections: string[]): void {
            this.application.isBusy = false;
        }

        get title(): string {
            return `Geräteübersicht`;
        }
    }
}