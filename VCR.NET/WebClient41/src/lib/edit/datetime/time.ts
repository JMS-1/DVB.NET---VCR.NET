/// <reference path="../edit.ts" />

namespace JMSLib.App {

    export interface ITime extends IConnectable {
        time: string;

        readonly error: string;
    }

    export class Time extends Property<string> implements ITime {
        private _time: string;

        protected onSiteChanged(): void {
            this._time = DateTimeUtils.formatEndTime(new Date(this.value));
        }

        constructor(data: any, prop: string, name?: string, onChange?: () => void, private _externalValidator?: () => string) {
            super(data, prop, name, onChange);
        }

        get time(): string {
            return this._time;
        }

        set time(newTime: string) {
            if (newTime !== this._time) {
                this._time = newTime;

                var parsed = DateTimeUtils.parseTime(newTime);

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
            else if ((this._time === undefined) || (DateTimeUtils.parseTime(this._time) !== null))
                super.validate();
            else
                this.message = "Ungültige Uhrzeit."
        }
    }
}