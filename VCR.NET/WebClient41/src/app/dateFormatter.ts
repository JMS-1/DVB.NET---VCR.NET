namespace VCRNETClient.App {
    // Formatiert Datum und Uhrzeit
    export class DateFormatter {
        // Die Kürzel für die Wochentage
        static germanDays: string[] = ['So', 'Mo', 'Di', 'Mi', 'Do', 'Fr', 'Sa'];

        // Stellt sicher, dass eine Zahl immer zweistellig ist
        static formatNumber(num: number): string {
            var asString = num.toString();
            if (asString.length > 1)
                return asString;
            else
                return `0${asString}`;
        }

        // Ermittelt die Uhrzeit
        static getEndTime(end: Date): string {
            return `${DateFormatter.formatNumber(end.getHours())}:${DateFormatter.formatNumber(end.getMinutes())}`;
        }

        // Ermittelt eine Dauer in Minuten und stellt diese als Uhrzeit dar
        static getDuration(duration: Date): string {
            return `${DateFormatter.formatNumber(duration.getUTCHours())}:${DateFormatter.formatNumber(duration.getUTCMinutes())}`;
        }

        // Ermittelt ein Datum
        static getStartDate(start: Date): string {
            return `${DateFormatter.getShortDate(start)}.${start.getFullYear().toString()}`;
        }

        // Ermittelt ein Datum ohne Jahresangabe
        static getShortDate(start: Date): string {
            return `${DateFormatter.germanDays[start.getDay()]} ${DateFormatter.formatNumber(start.getDate())}.${DateFormatter.formatNumber(1 + start.getMonth())}`;
        }

        // Ermittelt ein Datum ohne Jahresangabe
        static getShortDateUtc(start: Date): string {
            return `${DateFormatter.germanDays[start.getUTCDay()]} ${DateFormatter.formatNumber(start.getUTCDate())}.${DateFormatter.formatNumber(1 + start.getUTCMonth())}`;
        }

        // Ermittelt einen Startzeitpunkt
        static getStartTime(start: Date): string {
            var time = `${DateFormatter.formatNumber(start.getHours())}:${DateFormatter.formatNumber(start.getMinutes())}`;

            return `${DateFormatter.getStartDate(start)} ${time}`;
        }

        // Prüft eine Eingabe auf eine gültige Uhrzeit (H:M, jeweils ein oder zweistellig)
        static parseTime(time: string): number {
            var parts = time.split(':');
            if (parts.length != 2)
                return null;

            var hour = DateFormatter.parseHourMinute(parts[0]);
            if (hour == null)
                return null;
            if (hour > 23)
                return null;
            var minute = DateFormatter.parseHourMinute(parts[1]);
            if (minute == null)
                return null;
            if (minute > 59)
                return null;

            return (60 * hour + minute) * 60000;
        }

        // Analyisiert eine Eingabe auf eine gültige, maximal zweistellige nicht negative Zahl
        private static parseHourMinute(hourMinute: string): number {
            if (hourMinute.length == 1)
                hourMinute = '0' + hourMinute;
            if (hourMinute.length != 2)
                return null;

            var upper = DateFormatter.parseDigit(hourMinute.charCodeAt(0));
            if (upper == null)
                return null;
            var lower = DateFormatter.parseDigit(hourMinute.charCodeAt(1));
            if (lower == null)
                return null;

            return upper * 10 + lower;
        }

        // Anlysiert die Eingabe einer Ziffer
        private static parseDigit(digit: number): number {
            if (digit < 0x30)
                return null;
            if (digit > 0x39)
                return null;

            return digit - 0x30;
        }
    }
}