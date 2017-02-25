namespace VCRNETClient.App.Edit {

    // Schnittstelle zur Anzeige und zum Entfernen einer Ausnahmeregel.
    export interface IScheduleException {
        // Wird zurückgesetzt um die Ausnahmeregel beim Speichern zu entfernen.
        readonly isActive: JMSLib.App.IFlag;

        // Der Tag für den die Ausnahmeregel gilt.
        readonly dayDisplay: string;

        // Die Startverschiebung (in Minuten).
        readonly startShift: number;

        // Die Laufzeitänderung (in Minuten).
        readonly timeDelta: number;
    }

    // Präsentationsmodell zur Anzeige und zum Entfernen einer einzelnen Ausnahmeregel.
    export class ScheduleException implements IScheduleException {

        // Erstellt ein neues Präsentationsmodell.
        constructor(public readonly model: VCRServer.PlanExceptionContract, onChange: () => void) {
            this.isActive = new JMSLib.App.Flag({ value: true }, `value`, null, onChange);

            // Initiale Prüfungen ausführen.
            this.isActive.validate();
        }

        // Wird zurückgesetzt um die Ausnahmeregel beim Speichern zu entfernen.
        readonly isActive: JMSLib.App.Flag;

        // Der Tag für den die Ausnahmeregel gilt.
        get dayDisplay(): string {
            return JMSLib.App.DateTimeUtils.formatStartDate(new Date(parseInt(this.model.referenceDayDisplay, 10)));
        }

        // Die Startverschiebung (in Minuten).
        get startShift(): number {
            return this.model.startShift;
        }

        // Die Laufzeitänderung (in Minuten).
        get timeDelta(): number {
            return this.model.timeDelta;
        }
    }
}