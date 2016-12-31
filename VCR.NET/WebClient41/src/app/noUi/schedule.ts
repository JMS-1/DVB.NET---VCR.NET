﻿namespace VCRNETClient.App.NoUi {

    // Schnittstelle zur Pflege einer Aufzeichnung.
    export interface IScheduleEditor extends IJobScheduleEditor {
        // Datum der ersten Aufzeichnung.
        readonly firstStart: IDaySelector;

        // Laufzeit der Aufzeichnung.
        readonly duration: IDurationEditor;

        // Wiederholungsmuster
        readonly repeat: INumberEditor;

        readonly onMonday: IBooleanEditor;
        readonly onTuesday: IBooleanEditor;
        readonly onWednesday: IBooleanEditor;
        readonly onThursday: IBooleanEditor;
        readonly onFriday: IBooleanEditor;
        readonly onSaturday: IBooleanEditor;
        readonly onSunday: IBooleanEditor;
    }

    // Beschreibt die Daten einer Aufzeichnung.
    export class ScheduleEditor extends JobScheduleEditor<VCRServer.EditScheduleContract> implements IScheduleEditor {
        constructor(model: VCRServer.EditScheduleContract, favoriteSources: string[], onChange: () => void) {
            super(model, false, favoriteSources, onChange);

            // Pflegbare Eigenschaften anlegen.
            this.firstStart = new DayEditor(model, "firstStart", onChange, "Datum");
            this.repeat = new NumberEditor(model, "repeatPattern", onChange, "Wiederholung");
            this.duration = new DurationEditor(model, "firstStart", "duration", onChange, "Zeitraum");

            this.onMonday = new BooleanSetEditor(ScheduleEditor.flagMonday, this.repeat, DateFormatter.germanDays[1]);
            this.onTuesday = new BooleanSetEditor(ScheduleEditor.flagTuesday, this.repeat, DateFormatter.germanDays[2]);
            this.onWednesday = new BooleanSetEditor(ScheduleEditor.flagWednesday, this.repeat, DateFormatter.germanDays[3]);
            this.onThursday = new BooleanSetEditor(ScheduleEditor.flagThursday, this.repeat, DateFormatter.germanDays[4]);
            this.onFriday = new BooleanSetEditor(ScheduleEditor.flagFriday, this.repeat, DateFormatter.germanDays[5]);
            this.onSaturday = new BooleanSetEditor(ScheduleEditor.flagSaturday, this.repeat, DateFormatter.germanDays[6]);
            this.onSunday = new BooleanSetEditor(ScheduleEditor.flagSunday, this.repeat, DateFormatter.germanDays[0]);

            /*
            // Übernehmen
            this.exceptionInfos = $.map(rawData.exceptions, (rawException: VCRServer.PlanExceptionContract) => new PlanException(rawException));
            this.lastDay = (repeat == 0) ? ScheduleEditor.maximumDate : new Date(rawData.lastDay);
            */
        }

        // Datum der ersten Aufzeichnung.
        readonly firstStart: DayEditor;

        // Uhrzeit der ersten Aufzeichnung.
        readonly duration: DurationEditor;

        // Muster zur Wiederholung.
        readonly repeat: NumberEditor;

        // Der kleinste erlaubte Datumswert.
        static minimumDate: Date = new Date(1963, 8, 29);

        // Der höchste erlaubte Datumswert.
        static maximumDate: Date = new Date(2999, 11, 31);

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
            this.repeat.validate();
        }
    }

}