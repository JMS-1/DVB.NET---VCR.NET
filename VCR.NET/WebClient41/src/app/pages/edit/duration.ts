/// <reference path="../../../lib/edit/edit.ts" />

namespace VCRNETClient.App.Edit {

    // Schnittstelle zur Einstellung der Dauer einer Aufzeichnung.
    export interface IDurationEditor extends JMSLib.App.IDisplay {
        // Beginn (als Uhrzeit).
        readonly startTime: JMSLib.App.ITime;

        // Ende (als Uhrzeit).
        readonly endTime: JMSLib.App.ITime;
    }

    // Präsentationsmodell zur Eingabe der Dauer einer Aufzeichnung als Paar von Uhrzeiten.
    export class DurationEditor extends JMSLib.App.Property<number> implements IDurationEditor {

        // Beginn (als Uhrzeit).
        readonly startTime: JMSLib.App.Time;

        // Ende (als Uhrzeit).
        readonly endTime: JMSLib.App.Time;

        // Erstellt ein neues Präsentationsmodell.
        constructor(data: any, propTime: string, propDuration: string, text: string, onChange: () => void) {
            super(data, propDuration, text, onChange);

            // Die Startzeit ändert direkt den entsprechenden Wert in den Daten der Aufzeichnung.
            this.startTime = new JMSLib.App.Time(data, propTime, null, () => this.onChanged());

            // Aus der aktuellen Startzeit und der aktuellen Dauer das Ende ermitteln.
            var end = new Date(new Date(this.startTime.value).getTime() + 60000 * this.value);

            // Die Endzeit wird hier als absolute Zeit verwaltet.
            this.endTime = new JMSLib.App.Time({ value: end.toISOString() }, `value`, null, () => this.onChanged())
                .addValidator(t => this.checkLimit());

            // Initiale Prüfungen ausführen.
            this.startTime.validate();
            this.endTime.validate();
            this.validate();
        }

        // Wird bei jeder Eingabe von Start- oder Endzeit ausgelöst.
        private onChanged(): void {
            // Wir greifen hier direkt auf die Roheingaben zu - ansonsten müssten wir die Uhrzeit aus der tatsächlich verwalteten ISO Zeichenkette mühsam ermitteln.
            var start = JMSLib.App.DateTimeUtils.parseTime(this.startTime.rawValue);
            var end = JMSLib.App.DateTimeUtils.parseTime(this.endTime.rawValue);

            if ((start !== null) && (end !== null)) {
                // Die Dauer ist einfach die Differen aus Ende oder Start - liegt das Ende vor dem Start wird einfach nur von einem Tagessprung ausgegangen.
                var duration = (end - start) / 60000;
                if (duration <= 0)
                    duration += 24 * 60;

                // Das ist nun erst einmal der aktuelle Wert.
                this.value = duration;
            }

            // Auf jeden Fall die Änderung melden.
            this.refresh();
        }

        // Prüft die Dauer gegen die absolute Grenze.
        private checkLimit(): string {
            if (this.value >= 24 * 60)
                return "Die Aufzeichnungsdauer muss kleiner als ein Tag sein.";
        }

        // Prüft, ob die Eingabe der Dauer gültig ist.
        isValid(): boolean {
            // Wir haben keine eigene Fehlermeldung und mißbrauchen die Endzeit dafür.
            if (this.startTime.message !== ``)
                return false;
            if (this.endTime.message !== ``)
                return false;

            return true;
        }

    }
}