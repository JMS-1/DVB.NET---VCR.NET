namespace JMSLib.App {

    // Hilfsmethoden rund um Datum und Uhrzeit.
    export class DateTimeUtils {
        // Die Kürzel für die Wochentage.
        static readonly germanDays: string[] = ['So', 'Mo', 'Di', 'Mi', 'Do', 'Fr', 'Sa'];

        // Stellt sicher, dass eine Zahl immer zweistellig ist.
        static formatNumber(num: number): string {
            var asString = num.toString();
            if (asString.length > 1)
                return asString;
            else
                return `0${asString}`;
        }

        // Ermittelt die Uhrzeit.
        static formatEndTime(end: Date): string {
            return `${DateTimeUtils.formatNumber(end.getHours())}:${DateTimeUtils.formatNumber(end.getMinutes())}`;
        }

        // Ermittelt eine Dauer in Minuten und stellt diese als Uhrzeit dar.
        static formatDuration(duration: Date): string {
            return `${DateTimeUtils.formatNumber(duration.getUTCHours())}:${DateTimeUtils.formatNumber(duration.getUTCMinutes())}`;
        }

        // Ermittelt ein Datum.
        static formatStartDate(start: Date): string {
            return `${DateTimeUtils.formatShortDate(start)}.${start.getFullYear().toString()}`;
        }

        // Ermittelt ein Datum ohne Jahresangabe.
        static formatShortDate(start: Date): string {
            return `${DateTimeUtils.germanDays[start.getDay()]} ${DateTimeUtils.formatNumber(start.getDate())}.${DateTimeUtils.formatNumber(1 + start.getMonth())}`;
        }

        // Ermittelt ein Datum ohne Jahresangabe.
        static formatShortDateUtc(start: Date): string {
            return `${DateTimeUtils.germanDays[start.getUTCDay()]} ${DateTimeUtils.formatNumber(start.getUTCDate())}.${DateTimeUtils.formatNumber(1 + start.getUTCMonth())}`;
        }

        // Ermittelt einen Startzeitpunkt.
        static formatStartTime(start: Date): string {
            var time = `${DateTimeUtils.formatNumber(start.getHours())}:${DateTimeUtils.formatNumber(start.getMinutes())}`;

            return `${DateTimeUtils.formatStartDate(start)} ${time}`;
        }

        // Prüft eine Eingabe auf eine gültige Uhrzeit (H:M, jeweils ein oder zweistellig).
        static parseTime(time: string): number {
            var parts = time.split(':');
            if (parts.length != 2)
                return null;

            var hour = DateTimeUtils.parseHourMinute(parts[0]);
            if (hour == null)
                return null;
            if (hour > 23)
                return null;
            var minute = DateTimeUtils.parseHourMinute(parts[1]);
            if (minute == null)
                return null;
            if (minute > 59)
                return null;

            return (60 * hour + minute) * 60000;
        }

        // Analyisiert eine Eingabe auf eine gültige, maximal zweistellige nicht negative Zahl.
        private static parseHourMinute(hourMinute: string): number {
            if (hourMinute.length == 1)
                hourMinute = '0' + hourMinute;
            if (hourMinute.length != 2)
                return null;

            var upper = DateTimeUtils.parseDigit(hourMinute.charCodeAt(0));
            if (upper == null)
                return null;
            var lower = DateTimeUtils.parseDigit(hourMinute.charCodeAt(1));
            if (lower == null)
                return null;

            return upper * 10 + lower;
        }

        // Anlysiert die Eingabe einer Ziffer.
        private static parseDigit(digit: number): number {
            if (digit < 0x30)
                return null;
            if (digit > 0x39)
                return null;

            return digit - 0x30;
        }

        // Ermittelt aus einem Startzeitpunkt und einer Dauer die tatsächlich Dauer unter Berücksichtigung der Zeitumstellung.
        static getRealDurationInMinutes(isoStart: string, durationInMinutes: number): number {
            // Startzeitpunkt aus der ISO Notation in die lokale Zeit umrechnen.
            var start = new Date(isoStart);

            // Die ursprüngliche Dauer in der lokalen Zeit hinzufügen.
            var end = new Date(start.getFullYear(), start.getMonth(), start.getDate(), start.getHours(), start.getMinutes() + durationInMinutes);

            // Daraus kann nun die echte Zeitdifferenz ermittelt werden.
            return Math.floor((end.getTime() - start.getTime()) / 60000);
        }
    }
}