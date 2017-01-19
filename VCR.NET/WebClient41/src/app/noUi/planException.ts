namespace VCRNETClient.App.NoUi {

    // Erweiterte Schnittstelle zur Pflege einer einzelnen Ausnahmeregel.
    export interface IPlanException extends INoUiWithSite {
        // Die aktuelle Verschiebung des Startzeitpunktes in Minuten
        readonly startShift: number;

        // Die aktuelle Veränderung der Laufzeit in Minuten
        readonly timeDelta: number;

        // Der Regler zur Einstellung der Startzeitverschiebung.
        readonly startSlider: INumberSlider;

        // Der Regler zur Einstellung der Laufzeitveränderung.
        readonly durationSlider: INumberSlider;

        // Die Darstellung für den Zustand der Ausnahme.
        readonly exceptionMode: string;

        // Meldet den Startzeitpunkt als Text.
        getStart(): string;

        // Meldet den Endzeitpunkt als Text.
        getEnd(): string;

        // Meldet die aktuelle Dauer.
        getDuration(): number;

        // Verwendet die ursprüngliche Aufzeichnungsdaten.
        reset(): void;

        // Deaktiviert die Aufzeichnung vollständig.
        disable(): void;

        // Aktualisiert die Aufzeichnung.
        update(): void;
    }

    // Erweiterte Schnittstelle zur Pflege einer einzelnen Ausnahmeregel.
    export class PlanException implements IPlanException {
        constructor(exception: VCRServer.PlanExceptionContract, private _entryId: string, private _reload: () => void) {
            this.originalDuration = exception.originalDuration;
            this.referenceDay = exception.referenceDay;
            this.startShift = exception.startShift;
            this.timeDelta = exception.timeDelta;

            // Rohdaten wandeln.
            this.exceptionMode = ((exception.startShift !== 0) || (exception.timeDelta !== 0)) ? "exceptOn" : "exceptOff";
            this.referenceDayDisplay = parseInt(exception.referenceDayDisplay as string, 10);
            this.originalStart = new Date(exception.originalStart as string);

            // Editierfunktionen anbieten.
            this.startSlider = new NumberSlider(this, "startShift", () => this.refresh(), -480, +480);
            this.durationSlider = new NumberSlider(this, "timeDelta", () => this.refresh(), -this.originalDuration, +480);
        }

        // Der zugehörige Tag als interner Schlüssel, der unverändert zwischen Client und Service ausgetauscht wird
        referenceDay: string;

        // Der zugehörige Tag repräsentiert Date.getTime() Repräsentation
        referenceDayDisplay: number;

        // Die aktuelle Verschiebung des Startzeitpunktes in Minuten
        startShift: number;

        // Die aktuelle Veränderung der Laufzeit in Minuten
        timeDelta: number;

        // Der ursprüngliche Startzeitpunkt in ISO Notation
        originalStart: Date;

        // Die ursprüngliche Dauer in Minuten
        originalDuration: number;

        // Der Regler zur Einstellung der Startzeitverschiebung.
        readonly startSlider: NumberSlider;

        // Der Regler zur Einstellung der Laufzeitveränderung.
        readonly durationSlider: NumberSlider;

        // Die Darstellung für den Zustand der Ausnahme.
        readonly exceptionMode: string;

        // Meldet den Startzeitpunkt als Text.
        private start(): Date {
            return new Date((this.originalStart as Date).getTime() + 60 * this.startShift * 1000);
        }

        getStart(): string {
            return JMSLib.DateFormatter.getStartTime(this.start());
        }

        // Meldet den Endzeitpunkt als Text.
        private end(): Date {
            return new Date(this.start().getTime() + 60 * (this.originalDuration + this.timeDelta) * 1000);
        }

        getEnd(): string {
            return JMSLib.DateFormatter.getEndTime(this.end());
        }

        // Meldet die aktuelle Dauer.
        getDuration(): number {
            return this.originalDuration + this.timeDelta;
        }

        // Verwendet die ursprüngliche Aufzeichnungsdaten.
        reset(): void {
            this.startSlider.sync(0);
            this.durationSlider.sync(0);
        }

        // Deaktiviert die Aufzeichnung vollständig.
        disable(): void {
            this.startSlider.sync(0);
            this.durationSlider.sync(-this.originalDuration);
        }

        // Aktualisiert die Aufzeichnung.
        update(): void {
            VCRServer.updateException(this._entryId, this.referenceDay, this.startShift, this.timeDelta).then(this._reload);
        }

        // Beachrichtigungen einrichten.
        private m_site: INoUiSite;

        setSite(newSite: INoUiSite): void {
            this.m_site = newSite;
        }

        refresh(): void {
            if (this.m_site)
                this.m_site.refreshUi();
        }
    }
}