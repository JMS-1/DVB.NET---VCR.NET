namespace VCRNETClient.App.Edit {

    export interface IScheduleException extends JMSLib.App.IConnectable {
        readonly isActive: JMSLib.App.IFlag;

        readonly dayDisplay: string;

        readonly startShift: number;

        readonly timeDelta: number;
    }

    export class ScheduleException implements IScheduleException {
        constructor(public readonly model: VCRServer.PlanExceptionContract, onChange: () => void) {
            this.dayDisplay = JMSLib.App.DateFormatter.getStartDate(new Date(parseInt(model.referenceDayDisplay, 10)));

            this.isActive = new JMSLib.App.Flag(this, "_active", null, () => this.onChange(onChange));
        }

        private _active = true;

        site: JMSLib.App.ISite;

        readonly isActive: JMSLib.App.Flag;

        readonly dayDisplay: string;

        get startShift(): number {
            return this.model.startShift;
        }

        get timeDelta(): number {
            return this.model.timeDelta;
        }

        private onChange(onChange: () => void): void {
            onChange();

            if (this.site)
                this.site.refreshUi();
        }
    }
}