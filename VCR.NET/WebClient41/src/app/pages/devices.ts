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

    class DeviceInfo implements IDeviceInfo {

        constructor(private readonly _model: VCRServer.PlanCurrentContract) {
            if (!_model.isIdle) {
                var start = new Date(_model.start);
                var end = new Date(start.getTime() + _model.duration * 1000);

                this.start = JMSLib.DateFormatter.getStartTime(start);
                this.end = JMSLib.DateFormatter.getEndTime(end);
            }
        }

        get mode(): string {
            if (this._model.isIdle)
                return undefined;
            if (this._model.sourceName === 'PSI')
                return undefined;
            if (this._model.sourceName === 'EPG')
                return undefined;

            if (this._model.late)
                return 'late';
            else
                return 'intime';
        }

        get name(): string {
            return this._model.name;
        }

        readonly start: string;

        readonly end: string;

        get source(): string {
            return this._model.sourceName;
        }

        get device(): string {
            return this._model.device;
        }

        get size(): string {
            return this._model.size;
        }

        get id(): string {
            return this._model.id;
        }
    }

    export interface IDevicesPage extends IPage {
        readonly infos: IDeviceInfo[];
    }

    export class DevicesPage extends Page implements IDevicesPage {

        infos: DeviceInfo[] = [];

        constructor(application: Application) {
            super(`current`, application);
        }

        reset(sections: string[]): void {
            VCRServer.getPlanCurrent().then(plan => this.setPlan(plan));
        }

        private setPlan(plan: VCRServer.PlanCurrentContract[]): void {
            this.infos = (plan || []).map(info => new DeviceInfo(info));

            this.application.isBusy = false;
        }

        get title(): string {
            return `Geräteübersicht`;
        }
    }
}