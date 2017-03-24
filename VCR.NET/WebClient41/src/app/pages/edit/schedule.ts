/// <reference path="jobSchedule.ts" />

namespace VCRNETClient.App.Edit {

    // Schnittstelle zur Pflege einer Aufzeichnung.
    export interface IScheduleEditor extends IJobScheduleEditor {
        // Datum der ersten Aufzeichnung.
        readonly firstStart: JMSLib.App.IDaySelector;

        // Laufzeit der Aufzeichnung.
        readonly duration: IDurationEditor;

        // Wiederholungsmuster als Ganzes und aufgespalten als Wahrheitswert pro Wochentag.
        readonly repeat: JMSLib.App.INumber;

        readonly onMonday: JMSLib.App.IFlag;
        readonly onTuesday: JMSLib.App.IFlag;
        readonly onWednesday: JMSLib.App.IFlag;
        readonly onThursday: JMSLib.App.IFlag;
        readonly onFriday: JMSLib.App.IFlag;
        readonly onSaturday: JMSLib.App.IFlag;
        readonly onSunday: JMSLib.App.IFlag;

        // Ende der Wiederholung.
        readonly lastDay: JMSLib.App.IDaySelector;

        // Bekannte Ausnahmen der Wiederholungsregel.
        readonly hasExceptions: boolean;

        readonly exceptions: IScheduleException[];
    }

    // Beschreibt die Daten einer Aufzeichnung.
    export class ScheduleEditor extends JobScheduleEditor<VCRServer.EditScheduleContract> implements IScheduleEditor {

        // Erstellt ein neues Präsentationsmodell.
        constructor(page: IPage, model: VCRServer.EditScheduleContract, favoriteSources: string[], onChange: () => void, hasJobSource: () => boolean) {
            super(page, model, favoriteSources, onChange);

            // Anpassungen.
            if (!model.lastDay)
                model.lastDay = ScheduleEditor.maximumDate.toISOString();

            // Pflegbare Eigenschaften anlegen.
            this.repeat = new JMSLib.App.Number(model, "repeatPattern", "Wiederholung", () => this.onChange(onChange));
            this.duration = new DurationEditor(model, "firstStart", "duration", "Zeitraum", () => this.onChange(onChange));
            this.firstStart = new JMSLib.App.DayEditor(model, "firstStart", "Datum", onChange)
                .addValidator(() => this.validateFirstRecording());
            this.lastDay = new JMSLib.App.DayEditor(model, "lastDay", "wiederholen bis zum", () => this.onChange(onChange), true)
                .addRequiredValidator()
                .addValidator(day => ScheduleEditor.validateDateRange(day));

            this.onMonday = new JMSLib.App.FlagSet(ScheduleEditor.flagMonday, this.repeat, JMSLib.App.DateTimeUtils.germanDays[1]);
            this.onTuesday = new JMSLib.App.FlagSet(ScheduleEditor.flagTuesday, this.repeat, JMSLib.App.DateTimeUtils.germanDays[2]);
            this.onWednesday = new JMSLib.App.FlagSet(ScheduleEditor.flagWednesday, this.repeat, JMSLib.App.DateTimeUtils.germanDays[3]);
            this.onThursday = new JMSLib.App.FlagSet(ScheduleEditor.flagThursday, this.repeat, JMSLib.App.DateTimeUtils.germanDays[4]);
            this.onFriday = new JMSLib.App.FlagSet(ScheduleEditor.flagFriday, this.repeat, JMSLib.App.DateTimeUtils.germanDays[5]);
            this.onSaturday = new JMSLib.App.FlagSet(ScheduleEditor.flagSaturday, this.repeat, JMSLib.App.DateTimeUtils.germanDays[6]);
            this.onSunday = new JMSLib.App.FlagSet(ScheduleEditor.flagSunday, this.repeat, JMSLib.App.DateTimeUtils.germanDays[0]);

            // Ausnahmeregeln.
            this.exceptions = (model.exceptions || []).map(e => new ScheduleException(e, () => this.onExceptionsChanged()));

            // Zusätzliche Prüfung einrichten.
            this.source.sourceName.addValidator(c => {
                if (!hasJobSource())
                    if ((c.value || ``).trim().length < 1)
                        return `Entweder für die Aufzeichnung oder für den Auftrag muss eine Quelle angegeben werden.`;
            });

            // Initiale Prüfung.
            this.repeat.validate();
            this.lastDay.validate();
            this.firstStart.validate();
            this.source.sourceName.validate();
        }

        // Bei Änderungen an den Aufzeichnungsdaten muss eine übergreifende Gesamtprüfungen stattfinden, die wir an das Startdatum gebunden haben.
        private onChange(onOuterChange: () => void): void {
            // Gesamtprüfung anstossen.
            this.lastDay.validate();
            this.firstStart.validate();

            // Durchreichen.
            onOuterChange();
        }

        // Datum der ersten Aufzeichnung.
        readonly firstStart: JMSLib.App.DayEditor;

        // Uhrzeit der ersten Aufzeichnung.
        readonly duration: DurationEditor;

        // Muster zur Wiederholung.
        readonly repeat: JMSLib.App.Number;

        // Ende der Wiederholung
        readonly lastDay: JMSLib.App.DayEditor;

        // Bekannte Ausnahmen der Wiederholungsregel.
        get hasExceptions(): boolean {
            return (this.exceptions.length > 0);
        }

        readonly exceptions: ScheduleException[];

        // Hilfsmethode zum Arbeiten mit Datumswerten.
        private static makePureDate(date: Date): Date {
            return new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
        }

        // Der kleinste erlaubte Datumswert.
        static readonly minimumDate = ScheduleEditor.makePureDate(new Date(1963, 8, 29));

        // Der höchste erlaubte Datumswert.
        static readonly maximumDate = ScheduleEditor.makePureDate(new Date(2099, 11, 31));

        // Der höchste erlaubte Datumswert.
        private static readonly maximumDateLegacy = ScheduleEditor.makePureDate(new Date(2999, 11, 31));

        // Das Bit für Montag.
        static readonly flagMonday: number = 0x01;

        readonly onMonday: JMSLib.App.FlagSet;

        // Das Bit für Dienstag.
        static readonly flagTuesday: number = 0x02;

        readonly onTuesday: JMSLib.App.FlagSet;

        // Das Bit für Mittwoch.
        static readonly flagWednesday: number = 0x04;

        readonly onWednesday: JMSLib.App.FlagSet;

        // Das Bit für Donnerstag.
        static readonly flagThursday: number = 0x08;

        readonly onThursday: JMSLib.App.FlagSet;

        // Das Bit für Freitag.
        static readonly flagFriday: number = 0x10;

        readonly onFriday: JMSLib.App.FlagSet;

        // Das Bit für Samstag.
        static readonly flagSaturday: number = 0x20;

        readonly onSaturday: JMSLib.App.FlagSet;

        // Das Bit für Sonntag.
        static readonly flagSunday: number = 0x40;

        readonly onSunday: JMSLib.App.FlagSet;

        // Die Bitmasken aller Wochentage in der Ordnung von JavaScript (Date.getDay()).
        private static readonly _flags = [
            ScheduleEditor.flagSunday,
            ScheduleEditor.flagMonday,
            ScheduleEditor.flagTuesday,
            ScheduleEditor.flagWednesday,
            ScheduleEditor.flagThursday,
            ScheduleEditor.flagFriday,
            ScheduleEditor.flagSaturday
        ];

        // Prüft ob eon ausgewähltes Datum im unterstützten Bereich liegt.
        private static validateDateRange(day: JMSLib.App.DayEditor): string {
            var lastDay = new Date(day.value)

            if (lastDay < ScheduleEditor.minimumDate)
                return `Datum liegt zu weit in der Vergangenheit.`;
            else if (lastDay > ScheduleEditor.maximumDateLegacy)
                return `Datum liegt zu weit in der Zukunft.`;
        }

        // Prüft ob die Aufzeichnung überhaupt einmal stattfinden wird.
        private validateFirstRecording(): string {
            // Der letzte Tage einer Wiederholung.
            var lastDay = new Date(this.lastDay.value)

            // Geplanter erster (evt. einziger Start).
            var start = new Date(this.firstStart.value);

            // Die echte Dauer unter Berücksichtigung der Zeitumstellung ermitteln.
            var duration = JMSLib.App.DateTimeUtils.getRealDurationInMinutes(this.firstStart.value, this.duration.value);

            // Ende der ersten Aufzeichnung ermitteln - das sollte in den meisten Fällen schon passen.
            var end = new Date(start.getFullYear(), start.getMonth(), start.getDate(), start.getHours(), start.getMinutes() + duration);

            // Aktuelle Uhrzeit ermitteln.
            var now = new Date();

            // Ansonsten kann uns nur noch das Wiederholen retten.
            var repeat = this.repeat.value;

            if (repeat !== 0) {
                // Zur Vereinfachung der Vergleiche beginnen wir etwas vor dem aktuellen Tag.
                start = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 2, start.getHours(), start.getMinutes());

                // Von dort aus schauen wir in die Zukunft.
                for (; ;) {
                    // Den nächsten Wochentag suchen, an dem eine Wiederholung erlaubt ist.
                    do {
                        // Dabei den Startzeitpunkt immer um einen Tag vorrücken, bis es passt.
                        start = new Date(start.getFullYear(), start.getMonth(), start.getDate() + 1, start.getHours(), start.getMinutes());
                    }
                    while ((ScheduleEditor._flags[start.getDay()] & repeat) === 0)

                    // Dazu das eine Datum ermitteln - UTC, da auch unser Enddatum UTC ist.
                    var startDay = ScheduleEditor.makePureDate(start);

                    // Der Startzeitpunkt ist leider verboten.
                    if (startDay > lastDay)
                        break;

                    // Nun müssen wir uns das zugehörige Ende der Aufzeichnung anschauen.
                    var end = new Date(start.getFullYear(), start.getMonth(), start.getDate(), start.getHours(), start.getMinutes() + duration);

                    // Liegt dieses echt in der Zukunft ist alles gut.
                    if (end > now)
                        return;
                }
            }
            // Wenn die Aufzeichnung in der Zukunft endet ist alles gut.
            else if (end > now)
                return;

            // Die Aufzeichnung findet sicher niemals statt.
            return `Die Aufzeichnung liegt in der Vergangenheit.`;
        }

        // Gesetzt, wie die Daten der Aufzeichnung konsistent sind.
        isValid(): boolean {
            // Erst einmal die Basisklasse fragen.
            if (!super.isValid())
                return false;

            // Dann alle unseren eigenen Präsentationsmodelle.
            if (this.repeat.message)
                return false;
            if (this.firstStart.message)
                return false;
            if (this.repeat.value !== 0)
                if (this.lastDay.message)
                    return false;
            if (!this.duration.isValid())
                return false;

            return true;
        }

        // Die Liste der Ausnahmen wird immer mit aktualisiert.
        private onExceptionsChanged(): void {
            // Alle übernehmen, die nicht explizit zum Löschen deaktiviert wurden.
            this.model.exceptions = this.exceptions.filter(e => e.isActive.value).map(e => e.model);
        }

    }

}