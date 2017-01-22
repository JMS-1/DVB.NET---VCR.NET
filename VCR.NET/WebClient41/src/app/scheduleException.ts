namespace VCRNETClient.App {

    export interface IScheduleException extends JMSLib.App.IConnectable {
        readonly isActive: JMSLib.App.IValidateFlag;

        readonly dayDisplay: string;

        readonly startShift: number;

        readonly timeDelta: number;
    }

    export class ScheduleException implements IScheduleException {
        constructor(public readonly model: VCRServer.PlanExceptionContract, onChange: () => void) {
            this.dayDisplay = JMSLib.App.DateFormatter.getStartDate(new Date(parseInt(model.referenceDayDisplay, 10)));

            this.isActive = new JMSLib.App.FlagEditor(this, "_active", () => this.onChange(onChange), null);
        }

        private _active = true;

        private _site: JMSLib.App.ISite;

        setSite(newSite: JMSLib.App.ISite): void {
            this._site = newSite;
        }

        readonly isActive: JMSLib.App.FlagEditor;

        readonly dayDisplay: string;

        get startShift(): number {
            return this.model.startShift;
        }

        get timeDelta(): number {
            return this.model.timeDelta;
        }

        private onChange(onChange: () => void): void {
            onChange();

            if (this._site)
                this._site.refreshUi();
        }
    }
}