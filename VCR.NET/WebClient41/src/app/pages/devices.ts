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

        readonly showGuide: JMSLib.App.IFlag;

        readonly showControl: JMSLib.App.IFlag;
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
            var refresh = this.refresh.bind(this);

            this.infos = (plan || []).map(info => new Devices.Info(info, refresh));

            this.application.isBusy = false;
        }

        private _refreshing = false;

        private refresh(info: Devices.Info, guide: boolean): void {
            if (this._refreshing)
                return;

            this._refreshing = true;

            var flag = guide ? info.showGuide : info.showControl;
            var state = flag.value;

            this.infos.forEach(i => i.showControl.value = i.showGuide.value = false);

            flag.value = state;

            this._refreshing = false;

            this.refreshUi();
        }

        get title(): string {
            return `Geräteübersicht`;
        }
    }
}