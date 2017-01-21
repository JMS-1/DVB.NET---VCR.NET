/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    export interface ITimeEditor extends INoUiWithSite {
        time(newTime?: string): string;

        error(): string;
    }

    export class TimeEditor extends ValueHolderWithSite<string> implements ITimeEditor {
        private _time: string;

        protected onSiteChanged(): void {
            this._time = DateFormatter.getEndTime(new Date(this.value));
        }

        constructor(data: any, prop: string, onChange: () => void, name?: string, private _externalValidator?: () => string) {
            super(data, prop, onChange, name);
        }

        time(newTime?: string): string {
            var oldValue = this._time;

            if (newTime !== undefined)
                if (newTime !== oldValue) {
                    this._time = newTime;

                    var parsed = DateFormatter.parseTime(newTime);

                    if (parsed !== null) {
                        parsed /= 60000;

                        var oldDay = new Date(this.value);
                        var newDate = new Date(oldDay.getFullYear(), oldDay.getMonth(), oldDay.getDate(), Math.floor(parsed / 60), parsed % 60);

                        this.value = newDate.toISOString();
                    }

                    this.refresh();
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