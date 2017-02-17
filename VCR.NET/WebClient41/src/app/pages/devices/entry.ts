namespace VCRNETClient.App.Devices {

    export class Info implements IDeviceInfo {

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

            if (this._model.remainingMinutes > 0)
                return 'running';

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

}