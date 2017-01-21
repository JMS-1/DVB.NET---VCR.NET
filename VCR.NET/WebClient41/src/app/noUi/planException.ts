namespace VCRNETClient.App.NoUi {

    // Erweiterte Schnittstelle zur Pflege einer einzelnen Ausnahmeregel.
    export interface IPlanException extends INoUiWithSite {
        // Der Regler zur Einstellung der Startzeitverschiebung.
        readonly startSlider: INumberSlider;

        // Der Regler zur Einstellung der Laufzeitveränderung.
        readonly durationSlider: INumberSlider;

        // Die Darstellung für den Zustand der Ausnahme.
        readonly exceptionMode: string;

        // Meldet den Startzeitpunkt als Text.
        readonly currentStart: string;

        // Meldet den Endzeitpunkt als Text.
        readonly currentEnd: string;

        // Meldet die aktuelle Dauer.
        readonly currentDuration: number;

        // Verwendet die ursprüngliche Aufzeichnungsdaten.
        reset(): void;

        // Deaktiviert die Aufzeichnung vollständig.
        disable(): void;

        // Aktualisiert die Aufzeichnung.
        update(): void;
    }

    // Erweiterte Schnittstelle zur Pflege einer einzelnen Ausnahmeregel.
    export class PlanException implements IPlanException {
        constructor(private _exception: VCRServer.PlanExceptionContract, private _entryId: string, private _reload: () => void) {
            this._originalStart = new Date(_exception.originalStart as string);
            this.startSlider = new NumberSlider(_exception, "startShift", () => this.refresh(), -480, +480);
            this.durationSlider = new NumberSlider(_exception, "timeDelta", () => this.refresh(), -_exception.originalDuration, +480);
        }

        // Der ursprüngliche Startzeitpunkt
        private _originalStart: Date;

        // Der Regler zur Einstellung der Startzeitverschiebung.
        readonly startSlider: NumberSlider;

        // Der Regler zur Einstellung der Laufzeitveränderung.
        readonly durationSlider: NumberSlider;

        // Die Darstellung für den Zustand der Ausnahme.
        get exceptionMode(): string {
            return ((this._exception.startShift !== 0) || (this._exception.timeDelta !== 0)) ? "exceptOn" : "exceptOff";
        }

        // Meldet den Startzeitpunkt als Text.
        private start(): Date {
            return new Date(this._originalStart.getTime() + 60 * this._exception.startShift * 1000);
        }

        get currentStart(): string {
            return JMSLib.DateFormatter.getStartTime(this.start());
        }

        // Meldet den Endzeitpunkt als Text.
        private end(): Date {
            return new Date(this.start().getTime() + 60 * (this._exception.originalDuration + this._exception.timeDelta) * 1000);
        }

        get currentEnd(): string {
            return JMSLib.DateFormatter.getEndTime(this.end());
        }

        // Meldet die aktuelle Dauer.
        get currentDuration(): number {
            return this._exception.originalDuration + this._exception.timeDelta;
        }

        // Verwendet die ursprüngliche Aufzeichnungsdaten.
        reset(): void {
            this.startSlider.sync(0);
            this.durationSlider.sync(0);
        }

        // Deaktiviert die Aufzeichnung vollständig.
        disable(): void {
            this.startSlider.sync(0);
            this.durationSlider.sync(-this._exception.originalDuration);
        }

        // Aktualisiert die Aufzeichnung.
        update(): void {
            VCRServer.updateException(this._entryId, this._exception.referenceDay, this._exception.startShift, this._exception.timeDelta).then(this._reload);
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