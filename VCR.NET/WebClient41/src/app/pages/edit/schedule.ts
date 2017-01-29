/// <reference path="jobSchedule.ts" />

namespace VCRNETClient.App {

    // Schnittstelle zur Pflege einer Aufzeichnung.
    export interface IScheduleEditor extends IJobScheduleEditor {
        // Datum der ersten Aufzeichnung.
        readonly firstStart: JMSLib.App.IDaySelector;

        // Laufzeit der Aufzeichnung.
        readonly duration: IDurationEditor;

        // Wiederholungsmuster als Ganzes und aufgespalten als Wahrheitswert pro Wochentag.
        readonly repeat: JMSLib.App.IValidatedNumber;

        readonly onMonday: JMSLib.App.IValidatedFlag;
        readonly onTuesday: JMSLib.App.IValidatedFlag;
        readonly onWednesday: JMSLib.App.IValidatedFlag;
        readonly onThursday: JMSLib.App.IValidatedFlag;
        readonly onFriday: JMSLib.App.IValidatedFlag;
        readonly onSaturday: JMSLib.App.IValidatedFlag;
        readonly onSunday: JMSLib.App.IValidatedFlag;

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

            // Anpassungen.
            if (!model.lastDay)
                model.lastDay = ScheduleEditor.makePureDate(ScheduleEditor.maximumDate).toISOString();

            // Pflegbare Eigenschaften anlegen.
            this.firstStart = new JMSLib.App.DayEditor(model, "firstStart", onChange, "Datum", false);
            this.repeat = new JMSLib.App.EditNumber(model, "repeatPattern", onChange, "Wiederholung");
            this.lastDay = new JMSLib.App.DayEditor(model, "lastDay", onChange, "wiederholen bis zum", true);
            this.duration = new DurationEditor(model, "firstStart", "duration", onChange, "Zeitraum");

            this.onMonday = new JMSLib.App.EditFlagSet(ScheduleEditor.flagMonday, this.repeat, JMSLib.App.DateFormatter.germanDays[1]);
            this.onTuesday = new JMSLib.App.EditFlagSet(ScheduleEditor.flagTuesday, this.repeat, JMSLib.App.DateFormatter.germanDays[2]);
            this.onWednesday = new JMSLib.App.EditFlagSet(ScheduleEditor.flagWednesday, this.repeat, JMSLib.App.DateFormatter.germanDays[3]);
            this.onThursday = new JMSLib.App.EditFlagSet(ScheduleEditor.flagThursday, this.repeat, JMSLib.App.DateFormatter.germanDays[4]);
            this.onFriday = new JMSLib.App.EditFlagSet(ScheduleEditor.flagFriday, this.repeat, JMSLib.App.DateFormatter.germanDays[5]);
            this.onSaturday = new JMSLib.App.EditFlagSet(ScheduleEditor.flagSaturday, this.repeat, JMSLib.App.DateFormatter.germanDays[6]);
            this.onSunday = new JMSLib.App.EditFlagSet(ScheduleEditor.flagSunday, this.repeat, JMSLib.App.DateFormatter.germanDays[0]);

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
        static readonly maximumDate: Date = new Date(2099, 11, 31);

        // Das Bit für Montag.
        static readonly flagMonday: number = 0x01;

        readonly onMonday: JMSLib.App.EditFlagSet;

        // Das Bit für Dienstag.
        static readonly flagTuesday: number = 0x02;

        readonly onTuesday: JMSLib.App.EditFlagSet;

        // Das Bit für Mittwoch.
        static readonly flagWednesday: number = 0x04;

        readonly onWednesday: JMSLib.App.EditFlagSet;

        // Das Bit für Donnerstag.
        static readonly flagThursday: number = 0x08;

        readonly onThursday: JMSLib.App.EditFlagSet;

        // Das Bit für Freitag.
        static readonly flagFriday: number = 0x10;

        readonly onFriday: JMSLib.App.EditFlagSet;

        // Das Bit für Samstag.
        static readonly flagSaturday: number = 0x20;

        readonly onSaturday: JMSLib.App.EditFlagSet;

        // Das Bit für Sonntag.
        static readonly flagSunday: number = 0x40;

        readonly onSunday: JMSLib.App.EditFlagSet;

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