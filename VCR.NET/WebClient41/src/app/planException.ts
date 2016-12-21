namespace VCRNETClient.App {

    // Beschreibt einen einzelne Ausnahmeregel
    export class PlanException {
        constructor(rawData: VCRServer.PlanExceptionContract) {
            // Daten aus den Rohdaten übernehmen
            this.referenceDayDisplay = parseInt(rawData.referenceDayDisplay, 10);
            this.originalStart = new Date(rawData.originalStart);
            this.originalDuration = rawData.originalDuration;
            this.referenceDay = rawData.referenceDay;
            this.durationDelta = rawData.timeDelta;
            this.startDelta = rawData.startShift;
            this.rawException = rawData;
        }

        // Die Ausnahme, so wie der Web Service sie uns gemeldet hat.
        rawException: any;

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