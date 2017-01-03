namespace VCRNETClient.App {

    // Beschreibt einen einzelne Ausnahmeregel
    export class PlanException {
        constructor(public rawException: VCRServer.PlanExceptionContract) {
            // Daten aus den Rohdaten übernehmen
            this.referenceDayDisplay = parseInt(rawException.referenceDayDisplay, 10);
            this.originalStart = new Date(rawException.originalStart);
            this.originalDuration = rawException.originalDuration;
            this.referenceDay = rawException.referenceDay;
            this.durationDelta = rawException.timeDelta;
            this.startDelta = rawException.startShift;
        }

        // Das Referenzdatum für die Ausnahme.
        private referenceDay: string;

        // Das Referenzdatum für die Ausnahme.
        private referenceDayDisplay: number;

        // Der ursprüngliche Startzeitpunkt.
        private originalStart: Date;

        // Die geplante Dauer der Aufzeichnung.
        originalDuration: number;

        // Die Verschiebung des Startzeitpunktes in Minuten.
        startDelta: number;

        // Die Veränderung der Dauer in Minuten.
        durationDelta: number;

        // Gesetzt, wenn diese Ausnahme ausgewählt wurde.
        isActive: boolean;
    }

}