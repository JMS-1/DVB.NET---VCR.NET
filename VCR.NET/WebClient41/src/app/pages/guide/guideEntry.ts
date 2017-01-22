namespace VCRNETClient.App {

    export interface IGuideEntry {
        readonly startDisplay: string;

        readonly endDisplay: string;

        readonly source: string;

        readonly name: string;
    }

    export class GuideEntry implements IGuideEntry {
        constructor(private _model: VCRServer.GuideItemContract) {
            var start = new Date(_model.start);
            var end = new Date(start.getTime() + 1000 * _model.duration);

            this.startDisplay = JMSLib.DateFormatter.getStartTime(start);
            this.endDisplay = JMSLib.DateFormatter.getEndTime(end);
        }

        readonly startDisplay: string;

        readonly endDisplay: string;

        get source(): string {
            return this._model.station;
        }

        get name(): string {
            return this._model.name;
        }
    }
}