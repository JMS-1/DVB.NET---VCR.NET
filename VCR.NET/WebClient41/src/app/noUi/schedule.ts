namespace VCRNETClient.App.NoUi {

    // Schnittstelle zur Pflege einer Aufzeichnung.
    export interface IScheduleEditor extends IJobScheduleEditor {
        // Datum der ersten Aufzeichnung.
        readonly firstStart: IDaySelector;

        // Laufzeit der Aufzeichnung.
        readonly duration: IDurationEditor;
    }

    // Beschreibt die Daten einer Aufzeichnung.
    export class ScheduleEditor extends JobScheduleEditor<VCRServer.EditScheduleContract> implements IScheduleEditor {
        constructor(model: VCRServer.EditScheduleContract, favoriteSources: string[], onChange: () => void) {
            super(model, false, favoriteSources, onChange);

            // Pflegbare Eigenschaften anlegen.
            this.firstStart = new DayEditor(model, "firstStart", onChange);
            this.duration = new DurationEditor(model, "firstStart", "duration", onChange);

            /*
            var repeat = rawData.repeatPattern;
            var start = new Date(rawData.firstStart);
            var end = new Date(start.getTime() + 60000 * rawData.duration);

            // Übernehmen
            this.exceptionInfos = $.map(rawData.exceptions, (rawException: VCRServer.PlanExceptionContract) => new PlanException(rawException));
            this.lastDay = (repeat == 0) ? ScheduleEditor.maximumDate : new Date(rawData.lastDay);
            this.firstStart = new Date(start.getFullYear(), start.getMonth(), start.getDate());
            this.repeatWednesday = (repeat & ScheduleEditor.flagWednesday) != 0;
            this.repeatThursday = (repeat & ScheduleEditor.flagThursday) != 0;
            this.repeatSaturday = (repeat & ScheduleEditor.flagSaturday) != 0;
            this.repeatTuesday = (repeat & ScheduleEditor.flagTuesday) != 0;
            this.repeatMonday = (repeat & ScheduleEditor.flagMonday) != 0;
            this.repeatFriday = (repeat & ScheduleEditor.flagFriday) != 0;
            this.repeatSunday = (repeat & ScheduleEditor.flagSunday) != 0;
            this.startTime = DateFormatter.getEndTime(start);
            this.endTime = DateFormatter.getEndTime(end);
            */
        }

        // Datum der ersten Aufzeichnung.
        readonly firstStart: DayEditor;

        // Uhrzeit der ersten Aufzeichnung.
        readonly duration: DurationEditor;

        // Der kleinste erlaubte Datumswert.
        static minimumDate: Date = new Date(1963, 8, 29);

        // Der höchste erlaubte Datumswert.
        static maximumDate: Date = new Date(2999, 11, 31);

        // Das Bit für Montag.
        static flagMonday: number = 0x01;

        // Das Bit für Dienstag.
        static flagTuesday: number = 0x02;

        // Das Bit für Mittwoch.
        static flagWednesday: number = 0x04;

        // Das Bit für Donnerstag.
        static flagThursday: number = 0x08;

        // Das Bit für Freitag.
        static flagFriday: number = 0x10;

        // Das Bit für Samstag.
        static flagSaturday: number = 0x20;

        // Das Bit für Sonntag.
        static flagSunday: number = 0x40;

        validate(sources: VCRServer.SourceEntry[], sourceIsRequired: boolean): void {
            super.validate(sources, sourceIsRequired);

            this.firstStart.validate();
            this.duration.validate();
        }
    }

}