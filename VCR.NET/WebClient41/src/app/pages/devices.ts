/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IDeviceInfo extends JMSLib.App.IConnectable {
        readonly name: string;

        readonly displayStart: string;

        readonly displayEnd: string;

        readonly source: string;

        readonly device: string;

        readonly size: string;

        readonly mode: string;

        readonly id: string;

        readonly showGuide: JMSLib.App.IFlag;

        readonly showControl: JMSLib.App.IFlag;

        readonly guideItem: Guide.IGuideInfo;

        readonly guideTime: JMSLib.App.ITimeBar;

        readonly controller: IDeviceController;
    }

    export interface IDeviceController extends JMSLib.App.IConnectable {
        readonly end: string;

        readonly remaining: JMSLib.App.INumberWithSlider;

        readonly live: string;

        readonly timeshift: string;

        readonly stopNow: JMSLib.App.ICommand;

        readonly noHibernate: JMSLib.App.IFlag;

        readonly update: JMSLib.App.ICommand;
    }

    export interface IDevicesPage extends IPage {
        readonly infos: IDeviceInfo[];
    }

    export class DevicesPage extends Page implements IDevicesPage {

        infos: Devices.Info[] = [];

        constructor(application: Application) {
            super(`current`, application);

            this.navigation.refresh = true;
        }

        reset(sections: string[]): void {
            this.reload();
        }

        private setPlan(plan: VCRServer.PlanCurrentContract[]): void {
            var refresh = this.refresh.bind(this);
            var reload = this.reload.bind(this);

            this.infos = (plan || []).map(info => new Devices.Info(info, this.application.profile.suppressHibernate, refresh, reload));

            this.application.isBusy = false;

            this.refreshUi();
        }

        private _refreshing = false;

        reload(): void {
            VCRServer.getPlanCurrent().then(plan => this.setPlan(plan));
        }

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