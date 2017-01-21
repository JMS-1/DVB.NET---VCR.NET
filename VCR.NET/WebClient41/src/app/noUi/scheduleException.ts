namespace VCRNETClient.App.NoUi {

    export interface IScheduleException {
        readonly isActive: IBooleanEditor;

        readonly dayDisplay: string;

        readonly startShift: number;

        readonly timeDelta: number;
    }

    export class ScheduleException implements IScheduleException {
        constructor(public readonly model: VCRServer.PlanExceptionContract, onChange: () => void) {
            this.dayDisplay = DateFormatter.getStartDate(new Date(parseInt(model.referenceDayDisplay, 10)));

            this.isActive = new BooleanEditor(this, "_active", onChange, null);
        }

        private _active = true;

        readonly isActive: BooleanEditor;

        readonly dayDisplay: string;

        get startShift(): number {
            return this.model.startShift;
        }

        get timeDelta(): number {
            return this.model.timeDelta;
        }
    }
}