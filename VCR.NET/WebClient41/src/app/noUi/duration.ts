/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    export interface IDurationEditorSite {
        refresh(): void;
    }

    export interface IDurationEditor extends IDisplayText {
        setSite(site: IDurationEditorSite): void;

        readonly startTime: ITimeEditor;

        readonly endTime: ITimeEditor;
    }

    export class DurationEditor extends ValueHolder<number> implements IDurationEditor {
        private _site: ITimeEditorSite;

        setSite(site: ITimeEditorSite): void {
            this._site = site;

            if (!site)
                return;
        }

        readonly startTime: TimeEditor;

        readonly endTime: TimeEditor;

        constructor(data: any, propTime: string, propDuration: string, onChange: () => void, text: string) {
            super(data, propDuration, onChange, text);

            this.startTime = new TimeEditor(data, propTime, this._onChanged);

            var end = new Date(new Date(this.startTime.val()).getTime() + 60000 * this.val());

            this.endTime = new TimeEditor({ time: end.toISOString() }, "time", this._onChanged, this.checkLimit.bind(this));
        }

        private readonly _onChanged = this.onChanged.bind(this);

        private onChanged(): void {
            var start = DateFormatter.parseTime(this.startTime.time());

            if (start === null)
                return;

            var end = DateFormatter.parseTime(this.endTime.time());

            if (end === null)
                return;

            var duration = (end - start) / 60000;

            if (duration <= 0)
                duration += 24 * 60;

            this.val(duration);
        }

        private checkLimit(): string {
            return (this.val() >= 24 * 60) ? "Die Aufzeichnungsdauer muss kleiner als ein Tag sein." : undefined;
        }

        validate(): void {
            super.validate();

            this.startTime.validate();
            this.endTime.validate();

            if (this.message.length < 1)
                if (this.startTime.message.length > 0)
                    this.message = this.startTime.message;
                else
                    this.message = this.endTime.message;
        }
    }
}