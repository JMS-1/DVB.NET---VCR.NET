/// <reference path="../lib/edit.ts" />

namespace VCRNETClient.App {

    export interface ITimeEditor extends JMSLib.App.IConnectable {
        time: string;

        readonly error: string;
    }

    export class TimeEditor extends JMSLib.App.EditValueWithSite<string> implements ITimeEditor {
        private _time: string;

        protected onSiteChanged(): void {
            this._time = JMSLib.App.DateFormatter.getEndTime(new Date(this.value));
        }

        constructor(data: any, prop: string, onChange: () => void, name?: string, private _externalValidator?: () => string) {
            super(data, prop, onChange, name);
        }

        get time(): string {
            return this._time;
        }

        set time(newTime: string) {
            if (newTime !== this._time) {
                this._time = newTime;

                var parsed = JMSLib.App.DateFormatter.parseTime(newTime);

                if (parsed !== null) {
                    parsed /= 60000;

                    var oldDay = new Date(this.value);
                    var newDate = new Date(oldDay.getFullYear(), oldDay.getMonth(), oldDay.getDate(), Math.floor(parsed / 60), parsed % 60);

                    this.value = newDate.toISOString();
                }

                this.refresh();
            }
        }

        get error(): string {
            this.validate();

            return this.message;
        }

        validate(): void {
            var external = (this._externalValidator && this._externalValidator()) || "";

            if (external.length > 0)
                this.message = external;
            else if ((this._time === undefined) || (JMSLib.App.DateFormatter.parseTime(this._time) !== null))
                super.validate();
            else
                this.message = "Ungültige Uhrzeit."
        }
    }
}