namespace VCRNETClient.App.NoUi {

    // Schnittstelle zur Pflege einer Aufzeichnung.
    export interface IScheduleEditor extends IJobScheduleEditor {
        // Datum der ersten Aufzeichnung.
        readonly firstStart: IDaySelector;

        // Laufzeit der Aufzeichnung.
        readonly duration: IDurationEditor;

        // Wiederholungsmuster als Ganzes und aufgespalten als Wahrheitswert pro Wochentag.
        readonly repeat: INumberEditor;

        readonly onMonday: IBooleanEditor;
        readonly onTuesday: IBooleanEditor;
        readonly onWednesday: IBooleanEditor;
        readonly onThursday: IBooleanEditor;
        readonly onFriday: IBooleanEditor;
        readonly onSaturday: IBooleanEditor;
        readonly onSunday: IBooleanEditor;

        // Ende der Wiederholung.
        readonly lastDay: IDaySelector;

        // Bekannte Ausnahmen der Wiederholungsregel.
        readonly hasExceptions: boolean;

        readonly exceptions: IScheduleException[];
    }

    // Beschreibt die Daten einer Aufzeichnung.
    export class ScheduleEditor extends JobScheduleEditor<VCRServer.EditScheduleContract> implements IScheduleEditor {
        constructor(page: IPage, model: VCRServer.EditScheduleContract, favoriteSources: string[], onChange: () => void) {
            super(page, model, false, favoriteSources, onChange);

            // Pflegbare Eigenschaften anlegen.
            this.firstStart = new DayEditor(model, "firstStart", onChange, "Datum", false);
            this.repeat = new NumberEditor(model, "repeatPattern", onChange, "Wiederholung");
            this.lastDay = new DayEditor(model, "lastDay", onChange, "wiederholen bis zum", true);
            this.duration = new DurationEditor(model, "firstStart", "duration", onChange, "Zeitraum");

            this.onMonday = new BooleanSetEditor(ScheduleEditor.flagMonday, this.repeat, DateFormatter.germanDays[1]);
            this.onTuesday = new BooleanSetEditor(ScheduleEditor.flagTuesday, this.repeat, DateFormatter.germanDays[2]);
            this.onWednesday = new BooleanSetEditor(ScheduleEditor.flagWednesday, this.repeat, DateFormatter.germanDays[3]);
            this.onThursday = new BooleanSetEditor(ScheduleEditor.flagThursday, this.repeat, DateFormatter.germanDays[4]);
            this.onFriday = new BooleanSetEditor(ScheduleEditor.flagFriday, this.repeat, DateFormatter.germanDays[5]);
            this.onSaturday = new BooleanSetEditor(ScheduleEditor.flagSaturday, this.repeat, DateFormatter.germanDays[6]);
            this.onSunday = new BooleanSetEditor(ScheduleEditor.flagSunday, this.repeat, DateFormatter.germanDays[0]);

            // Ausnahmeregeln.
            this.exceptions = (model.exceptions || []).map(e => new ScheduleException(e, () => this.onExceptionsChanged()));
            this.hasExceptions = (this.exceptions.length > 0);
        }

        // Datum der ersten Aufzeichnung.
        readonly firstStart: DayEditor;

        // Uhrzeit der ersten Aufzeichnung.
        readonly duration: DurationEditor;

        // Muster zur Wiederholung.
        readonly repeat: NumberEditor;

        // Ende der Wiederholung
        readonly lastDay: DayEditor;

        // Bekannte Ausnahmen der Wiederholungsregel.
        readonly hasExceptions: boolean;

        readonly exceptions: ScheduleException[];

        // Hilfsmethode zum Arbeiten mit Datumswerten.
        public static makePureDate(date: Date): Date {
            return new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
        }

        // Der kleinste erlaubte Datumswert.
        static readonly minimumDate: Date = new Date(1963, 8, 29);

        // Der höchste erlaubte Datumswert.
        static readonly maximumDate: Date = new Date(2999, 11, 31);

        // Das Bit für Montag.
        private static flagMonday: number = 0x01;

        readonly onMonday: BooleanSetEditor;

        // Das Bit für Dienstag.
        private static flagTuesday: number = 0x02;

        readonly onTuesday: BooleanSetEditor;

        // Das Bit für Mittwoch.
        private static flagWednesday: number = 0x04;

        readonly onWednesday: BooleanSetEditor;

        // Das Bit für Donnerstag.
        private static flagThursday: number = 0x08;

        readonly onThursday: BooleanSetEditor;

        // Das Bit für Freitag.
        private static flagFriday: number = 0x10;

        readonly onFriday: BooleanSetEditor;

        // Das Bit für Samstag.
        private static flagSaturday: number = 0x20;

        readonly onSaturday: BooleanSetEditor;

        // Das Bit für Sonntag.
        private static flagSunday: number = 0x40;

        readonly onSunday: BooleanSetEditor;

        validate(sources: VCRServer.SourceEntry[], sourceIsRequired: boolean): void {
            super.validate(sources, sourceIsRequired);

            this.firstStart.validate();
            this.duration.validate();
            this.lastDay.validate();
            this.repeat.validate();
        }

        isValid(): boolean {
            if (!super.isValid())
                return false;
            if (this.firstStart.message.length > 0)
                return false;
            if (this.duration.message.length > 0)
                return false;
            if (this.repeat.message.length > 0)
                return false;
            if (this.lastDay.message.length > 0)
                return false;

            return true;
        }

        private onExceptionsChanged(): void {
            this.model.exceptions = this.exceptions.filter(e => e.isActive.value).map(e => e.model);
        }
    }

}