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

        readonly createNew: JMSLib.App.ICommand;
    }

    export class GuideEntry implements IGuideEntry {
        constructor(public readonly model: VCRServer.GuideItemContract, private _toggleDetails: (entry: GuideEntry) => void, createNew: (entry: GuideEntry) => void, public jobSelector: JMSLib.App.IValidateStringFromList) {
            var start = new Date(model.start);
            var end = new Date(start.getTime() + 1000 * model.duration);

            this.startDisplay = JMSLib.DateFormatter.getStartTime(start);
            this.endDisplay = JMSLib.DateFormatter.getEndTime(end);

            this.createNew = new JMSLib.App.Command(() => createNew(this), "Aufzeichnung anlegen", () => end > new Date());
        }

        readonly createNew: JMSLib.App.ICommand;

        readonly startDisplay: string;

        readonly endDisplay: string;

        showDetails = false;

        get source(): string {
            return this.model.station;
        }

        get name(): string {
            return this.model.name;
        }

        toggleDetail(): void {
            this._toggleDetails(this);
        }

        get language(): string {
            return this.model.language;
        }

        get shortDescription(): string {
            return this.model.shortDescription;
        }

        get longDescription(): string {
            return this.model.description;
        }

        get duration(): string {
            return JMSLib.DateFormatter.getDuration(new Date(1000 * this.model.duration));
        }

        get rating(): string {
            return (this.model.ratings || []).join(" ");
        }

        get content(): string {
            return (this.model.categories || []).join(" ");
        }

    }
}