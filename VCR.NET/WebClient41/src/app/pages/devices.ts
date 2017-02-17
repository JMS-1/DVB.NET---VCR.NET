/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IDeviceInfo {
        readonly name: string;

        readonly start: string;

        readonly end: string;

        readonly source: string;

        readonly device: string;

        readonly size: string;

        readonly mode: string;

        readonly id: string;
    }

    export interface IDevicesPage extends IPage {
        readonly infos: IDeviceInfo[];
    }

    export class DevicesPage extends Page implements IDevicesPage {

        infos: Devices.Info[] = [];

        constructor(application: Application) {
            super(`current`, application);
        }

        reset(sections: string[]): void {
            VCRServer.getPlanCurrent().then(plan => this.setPlan(plan));
        }

        private setPlan(plan: VCRServer.PlanCurrentContract[]): void {
            this.infos = (plan || []).map(info => new Devices.Info(info));

            this.application.isBusy = false;
        }

        get title(): string {
            return `Geräteübersicht`;
        }
    }
}