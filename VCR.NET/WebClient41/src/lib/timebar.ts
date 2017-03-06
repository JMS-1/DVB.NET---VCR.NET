namespace JMSLib.App {

    // Schnittstelle zur Anzeige einer Zeitschiene.
    export interface ITimeBar {
        // Vorlaufzeit in Prozent.
        readonly prefixTime: number;

        // Dauer in Prozent.
        readonly time: number;

        // Nachlaufzeit in Prozent.
        readonly suffixTime: number;

        // Aktuelle Uhrzeit in prozent.
        readonly currentTime?: number;

        // Gesetzt, wenn die Dauer vollständig sichtbar ist.
        readonly timeIsComplete: boolean;

    }

    // Präsentationsmodell zur Darstellung einer Zeitschiene.
    export class TimeBar implements ITimeBar {

        // Erstellt ein neues Präsentationsmodell.
        constructor(startRecording: Date, endRecording: Date, startProgram: Date, endProgram: Date) {
            // Prüfen, ob die gesamte Dauer angezeigt werden kann.
            this.timeIsComplete = ((startRecording <= startProgram) && (endRecording >= endProgram))

            // Die tatsächliche Dauer.
            var total = endRecording.getTime() - startRecording.getTime();

            // Prozentuale Vorlaufzeit berechnen.
            var prefixTime = startProgram.getTime() - startRecording.getTime();

            if (prefixTime <= 0)
                this.prefixTime = 0;
            else
                this.prefixTime = Math.floor(prefixTime * 100 / total);

            // Prozentuale Nachlaufzeit berechnen.
            var suffixTime = endRecording.getTime() - endProgram.getTime();

            if (suffixTime <= 0)
                this.suffixTime = 0;
            else
                this.suffixTime = Math.floor(suffixTime * 100 / total);

            // Prozentuale Dauer berechnen.
            this.time = Math.max(0, Math.min(100, 100 - (this.prefixTime + this.suffixTime)));

            // Aktuelle Uhrzeit einblenden, wenn möglich.
            var currentTime = Date.now() - startRecording.getTime();

            if ((currentTime >= 0) && (currentTime <= total))
                this.currentTime = Math.floor(currentTime * 100 / total);
        }

        // Vorlauf in Prozent.
        readonly prefixTime: number;

        // Dauer in Prozent.
        readonly time: number;

        // Gesetzt, wenn die Dauer vollständig angezeigt wird.
        readonly timeIsComplete: boolean;

        // Nachlauf in Prozent.
        readonly suffixTime: number;

        // Optional die aktuelle Zeit.
        readonly currentTime?: number;
    }
}