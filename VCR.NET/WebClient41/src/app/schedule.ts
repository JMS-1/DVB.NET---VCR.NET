namespace VCRNETClient.App {

    // Schnittstelle zur Pflege einer Aufzeichnung.
    export interface IScheduleEditor extends IJobScheduleEditor {
        // Datum der ersten Aufzeichnung.
        readonly firstStart: JMSLib.App.IDaySelector;

        // Laufzeit der Aufzeichnung.
        readonly duration: IDurationEditor;

        // Wiederholungsmuster als Ganzes und aufgespalten als Wahrheitswert pro Wochentag.
        readonly repeat: JMSLib.App.IValidatedNumber;

        readonly onMonday: JMSLib.App.IValidateFlag;
        readonly onTuesday: JMSLib.App.IValidateFlag;
        readonly onWednesday: JMSLib.App.IValidateFlag;
        readonly onThursday: JMSLib.App.IValidateFlag;
        readonly onFriday: JMSLib.App.IValidateFlag;
        readonly onSaturday: JMSLib.App.IValidateFlag;
        readonly onSunday: JMSLib.App.IValidateFlag;

        // Ende der Wiederholung.
        readonly lastDay: JMSLib.App.IDaySelector;

        // Bekannte Ausnahmen der Wiederholungsregel.
        readonly hasExceptions: boolean;

        readonly exceptions: IScheduleException[];
    }

    // Beschreibt die Daten einer Aufzeichnung.
    export class ScheduleEditor extends JobScheduleEditor<VCRServer.EditScheduleContract> implements IScheduleEditor {
        constructor(page: IPage, model: VCRServer.EditScheduleContract, favoriteSources: string[], onChange: () => void) {
            super(page, model, false, favoriteSources, onChange);

            // Pflegbare Eigenschaften anlegen.
            this.firstStart = new JMSLib.App.DayEditor(model, "firstStart", onChange, "Datum", false);
            this.repeat = new JMSLib.App.EditNumber(model, "repeatPattern", onChange, "Wiederholung");
            this.lastDay = new JMSLib.App.DayEditor(model, "lastDay", onChange, "wiederholen bis zum", true);
            this.duration = new DurationEditor(model, "firstStart", "duration", onChange, "Zeitraum");

            this.onMonday = new JMSLib.App.FlagSetEditor(ScheduleEditor.flagMonday, this.repeat, JMSLib.App.DateFormatter.germanDays[1]);
            this.onTuesday = new JMSLib.App.FlagSetEditor(ScheduleEditor.flagTuesday, this.repeat, JMSLib.App.DateFormatter.germanDays[2]);
            this.onWednesday = new JMSLib.App.FlagSetEditor(ScheduleEditor.flagWednesday, this.repeat, JMSLib.App.DateFormatter.germanDays[3]);
            this.onThursday = new JMSLib.App.FlagSetEditor(ScheduleEditor.flagThursday, this.repeat, JMSLib.App.DateFormatter.germanDays[4]);
            this.onFriday = new JMSLib.App.FlagSetEditor(ScheduleEditor.flagFriday, this.repeat, JMSLib.App.DateFormatter.germanDays[5]);
            this.onSaturday = new JMSLib.App.FlagSetEditor(ScheduleEditor.flagSaturday, this.repeat, JMSLib.App.DateFormatter.germanDays[6]);
            this.onSunday = new JMSLib.App.FlagSetEditor(ScheduleEditor.flagSunday, this.repeat, JMSLib.App.DateFormatter.germanDays[0]);

            // Ausnahmeregeln.
            this.exceptions = (model.exceptions || []).map(e => new ScheduleException(e, () => this.onExceptionsChanged()));
            this.hasExceptions = (this.exceptions.length > 0);
        }

        // Datum der ersten Aufzeichnung.
        readonly firstStart: JMSLib.App.DayEditor;

        // Uhrzeit der ersten Aufzeichnung.
        readonly duration: DurationEditor;

        // Muster zur Wiederholung.
        readonly repeat: JMSLib.App.EditNumber;

        // Ende der Wiederholung
        readonly lastDay: JMSLib.App.DayEditor;

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

        readonly onMonday: JMSLib.App.FlagSetEditor;

        // Das Bit für Dienstag.
        private static flagTuesday: number = 0x02;

        readonly onTuesday: JMSLib.App.FlagSetEditor;

        // Das Bit für Mittwoch.
        private static flagWednesday: number = 0x04;

        readonly onWednesday: JMSLib.App.FlagSetEditor;

        // Das Bit für Donnerstag.
        private static flagThursday: number = 0x08;

        readonly onThursday: JMSLib.App.FlagSetEditor;

        // Das Bit für Freitag.
        private static flagFriday: number = 0x10;

        readonly onFriday: JMSLib.App.FlagSetEditor;

        // Das Bit für Samstag.
        private static flagSaturday: number = 0x20;

        readonly onSaturday: JMSLib.App.FlagSetEditor;

        // Das Bit für Sonntag.
        private static flagSunday: number = 0x40;

        readonly onSunday: JMSLib.App.FlagSetEditor;

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