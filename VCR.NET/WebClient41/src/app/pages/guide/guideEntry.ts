namespace VCRNETClient.App {

    export interface IGuideEntry {
        readonly startDisplay: string;

        readonly endDisplay: string;

        readonly source: string;

        readonly name: string;

        readonly showDetails: boolean;

        readonly language: string;

        readonly duration: string;

        readonly rating: string;

        readonly content: string;

        readonly shortDescription: string;

        readonly longDescription: string;

        toggleDetail(): void;

        readonly jobSelector: JMSLib.App.IValidateStringFromList;
    }

    export class GuideEntry implements IGuideEntry {
        constructor(private _model: VCRServer.GuideItemContract, private _toggleDetails: (entry: GuideEntry) => void, public jobSelector: JMSLib.App.IValidateStringFromList) {
            var start = new Date(_model.start);
            var end = new Date(start.getTime() + 1000 * _model.duration);

            this.startDisplay = JMSLib.DateFormatter.getStartTime(start);
            this.endDisplay = JMSLib.DateFormatter.getEndTime(end);
        }

        readonly startDisplay: string;

        readonly endDisplay: string;

        showDetails = false;

        get source(): string {
            return this._model.station;
        }

        get name(): string {
            return this._model.name;
        }

        toggleDetail(): void {
            this._toggleDetails(this);
        }

        get language(): string {
            return this._model.language;
        }

        get shortDescription(): string {
            return this._model.shortDescription;
        }

        get longDescription(): string {
            return this._model.description;
        }

        get duration(): string {
            return JMSLib.DateFormatter.getDuration(new Date(1000 * this._model.duration));
        }

        get rating(): string {
            return (this._model.ratings || []).join(" ");
        }

        get content(): string {
            return (this._model.categories || []).join(" ");
        }

    }
}