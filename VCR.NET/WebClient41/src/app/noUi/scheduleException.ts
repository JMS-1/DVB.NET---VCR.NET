namespace VCRNETClient.App.NoUi {

    export interface IScheduleException extends INoUiWithSite {
        readonly isActive: IBooleanEditor;

        readonly dayDisplay: string;

        readonly startShift: number;

        readonly timeDelta: number;
    }

    export class ScheduleException implements IScheduleException {
        constructor(public readonly model: VCRServer.PlanExceptionContract, onChange: () => void) {
            this.dayDisplay = DateFormatter.getStartDate(new Date(parseInt(model.referenceDayDisplay, 10)));

            this.isActive = new BooleanEditor(this, "_active", () => this.onChange(onChange), null);
        }

        private _active = true;

        private _site: INoUiSite;

        setSite(newSite: INoUiSite): void {
            this._site = newSite;
        }

        readonly isActive: BooleanEditor;

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