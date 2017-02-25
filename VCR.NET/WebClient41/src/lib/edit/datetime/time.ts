/// <reference path="../edit.ts" />

namespace JMSLib.App {

    // Schnittstelle zur Eingabe einer Uhrzeit.
    export interface ITime extends IConnectable {
        // Die aktuelle Zeit als Zeichenkette.
        rawValue: string;

        // Optional eine zugehörige Fehlermeldung.
        readonly message: string;
    }

    // Erlaubt die Eingabe einer Uhrzeit - tatsächlich gespeichert wird ein volles Datum mit Uhrezit.
    export class Time extends Property<string> implements ITime {

        // Die aktuelle Uhrzeit.
        private _rawValue: string;

        // Wird ausgelöst, wenn sich ein neues Oberflächenelement zur Pflege der Uhrzeit verbindet.
        protected onSiteChanged(): void {
            this._rawValue = DateTimeUtils.formatEndTime(new Date(this.value));
        }

        // Erstellt ein neues Präsentationsmodell.
        constructor(data?: any, prop?: string, name?: string, onChange?: () => void) {
            super(data, prop, name, onChange);

            // Syntaxprüfung durchführen.
            this.addValidator(Time.isValidTime);
        }

        // Prüft, ob eine gültige Uhrzeit eingegeben wurde.
        private static isValidTime(time: Time): string {
            if (time._rawValue !== undefined)
                if (DateTimeUtils.parseTime(time._rawValue) === null)
                    return `Ungültige Uhrzeit.`;
        }

        // Meldet die aktuelle Uhrzeit.
        get rawValue(): string {
            return this._rawValue;
        }

        // Ändert die aktuelle Uhrzeit.
        set rawValue(newTime: string) {
            // Es hat sich gar keine Änderung ergeben.
            if (newTime === this._rawValue)
                return;

            // Die tatsächliche Eingabe des Anwenders.
            this._rawValue = newTime;

            // Schauen wir mal, ob das dem Format (H:M) entspricht.
            var parsed = DateTimeUtils.parseTime(newTime);

            // Wenn ja, erhalten wir das Ergebnis in Millisekunden - so wie man das zum Rechnen mit Zeitebn eigentlich braucht.
            if (parsed !== null) {
                // Umrechnen in Minuten.
                parsed /= 60000;

                // Uhrzeit einmischen.
                var oldDay = new Date(this.value);
                var newDate = new Date(oldDay.getFullYear(), oldDay.getMonth(), oldDay.getDate(), Math.floor(parsed / 60), parsed % 60);

                // Vollständiges Datum mit Uhrzeit aktualisieren.
                this.value = newDate.toISOString();
            }

            // Anzeige aktualisieren.
            this.refresh();
        }
    }
}