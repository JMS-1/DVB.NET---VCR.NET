namespace VCRNETClient.App.Plan {

    // Erweiterte Schnittstelle zur Pflege einer einzelnen Ausnahmeregel.
    export interface IPlanException extends JMSLib.App.IConnectable {
        // Der Regler zur Einstellung der Startzeitverschiebung.
        readonly startSlider: JMSLib.App.IEditNumberWithSlider;

        // Der Regler zur Einstellung der Laufzeitveränderung.
        readonly durationSlider: JMSLib.App.IEditNumberWithSlider;

        // Die Darstellung für den Zustand der Ausnahme.
        readonly exceptionMode: string;

        // Meldet den Startzeitpunkt als Text.
        readonly currentStart: string;

        // Meldet den Endzeitpunkt als Text.
        readonly currentEnd: string;

        // Meldet die aktuelle Dauer.
        readonly currentDuration: number;

        // Verwendet die ursprüngliche Aufzeichnungsdaten.
        setToOriginal(): void;

        // Deaktiviert die Aufzeichnung vollständig.
        setToDisable(): void;

        // Aktualisiert die Aufzeichnung.
        update(): void;
    }

    // Erweiterte Schnittstelle zur Pflege einer einzelnen Ausnahmeregel.
    export class PlanException implements IPlanException {
        constructor(private model: VCRServer.PlanExceptionContract, private _entryId: string, private _reload: () => void) {
            this._originalStart = new Date(model.originalStart as string);
            this.startSlider = new JMSLib.App.EditNumberWithSlider(model, "startShift", () => this.refresh(), -480, +480);
            this.durationSlider = new JMSLib.App.EditNumberWithSlider(model, "timeDelta", () => this.refresh(), -model.originalDuration, +480);
        }

        // Der ursprüngliche Startzeitpunkt
        private _originalStart: Date;

        // Der Regler zur Einstellung der Startzeitverschiebung.
        readonly startSlider: JMSLib.App.EditNumberWithSlider;

        // Der Regler zur Einstellung der Laufzeitveränderung.
        readonly durationSlider: JMSLib.App.EditNumberWithSlider;

        // Die Darstellung für den Zustand der Ausnahme.
        get exceptionMode(): string {
            return ((this.model.startShift !== 0) || (this.model.timeDelta !== 0)) ? "exceptOn" : "exceptOff";
        }

        // Meldet den Startzeitpunkt als Text.
        private start(): Date {
            return new Date(this._originalStart.getTime() + 60 * this.model.startShift * 1000);
        }

        get currentStart(): string {
            return JMSLib.DateFormatter.getStartTime(this.start());
        }

        // Meldet den Endzeitpunkt als Text.
        private end(): Date {
            return new Date(this.start().getTime() + 60 * (this.model.originalDuration + this.model.timeDelta) * 1000);
        }

        get currentEnd(): string {
            return JMSLib.DateFormatter.getEndTime(this.end());
        }

        // Meldet die aktuelle Dauer.
        get currentDuration(): number {
            return this.model.originalDuration + this.model.timeDelta;
        }

        // Setzt alles auf den Eingangszustand zurück.
        reset(): void {
            this.startSlider.reset()
            this.durationSlider.reset();
        }

        // Verwendet die ursprüngliche Aufzeichnungsdaten.
        setToOriginal(): void {
            this.startSlider.sync(0);
            this.durationSlider.sync(0);
        }

        // Deaktiviert die Aufzeichnung vollständig.
        setToDisable(): void {
            this.startSlider.sync(0);
            this.durationSlider.sync(-this.model.originalDuration);
        }

        // Aktualisiert die Aufzeichnung.
        update(): void {
            VCRServer.updateException(this._entryId, this.model.referenceDay, this.model.startShift, this.model.timeDelta).then(this._reload);
        }

        // Beachrichtigungen einrichten.
        private m_site: JMSLib.App.ISite;

        setSite(newSite: JMSLib.App.ISite): void {
            this.m_site = newSite;
        }

        refresh(): void {
            if (this.m_site)
                this.m_site.refreshUi();
        }
    }
}