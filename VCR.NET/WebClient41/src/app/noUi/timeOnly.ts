/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    export interface ITimeEditorSite {
        refresh(): void;
    }

    export interface ITimeEditor {
        setSite(site: ITimeEditorSite): void;

        time(newTime?: string): string;

        error(): string;
    }

    export class TimeEditor extends ValueHolder<string> implements ITimeEditor {
        private _time: string;

        private _site: ITimeEditorSite;

        setSite(site: ITimeEditorSite): void {
            this._site = site;

            if (!site)
                return;

            this._time = DateFormatter.getEndTime(new Date(this.val()));
        }

        constructor(data: any, prop: string, onChange: () => void, private _externalValidator?: () => string) {
            super(data, prop, onChange);
        }

        time(newTime?: string): string {
            var oldValue = this._time;

            if (newTime !== undefined)
                if (newTime !== oldValue) {
                    this._time = newTime;

                    var parsed = DateFormatter.parseTime(newTime);

                    if (parsed !== null) {
                        parsed /= 60000;

                        var oldDay = new Date(this.val());
                        var newDate = new Date(oldDay.getFullYear(), oldDay.getMonth(), oldDay.getDate(), Math.floor(parsed / 60), parsed % 60);

                        this.val(newDate.toISOString());
                    }

                    if (this._site)
                        this._site.refresh();
                }

            return oldValue;
        }

        error(): string {
            this.validate();

            return this.message;
        }

        validate(): void {
            var external = (this._externalValidator && this._externalValidator()) || "";

            if (external.length > 0)
                this.message = external;
            else if ((this._time === undefined) || (DateFormatter.parseTime(this._time) !== null))
                super.validate();
            else
                this.message = "Ungültige Uhrzeit."
        }
    }
}