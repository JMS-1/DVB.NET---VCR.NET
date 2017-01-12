namespace VCRNETClient.App.NoUi {

    // Erweiterte Schnittstelle zur Pflege einer einzelnen Ausnahmeregel.
    export interface IPlanException extends VCRServer.PlanExceptionContract, INoUiWithSite {
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

    // Interne Schnittstelle zur Pflege der nach außen nur lesbaren Felder-.
    interface IPlanExceptionEx extends IPlanException {
        // Der Regler zur Einstellung der Startzeitverschiebung.
        startSlider: NumberSlider;

        // Der Regler zur Einstellung der Laufzeitveränderung.
        durationSlider: NumberSlider;

        // Die Darstellung für den Zustand der Ausnahme.
        exceptionMode: string;
    }

    // Bereitet die Rohdaten zur Pflege im Aufzeichnungsplan auf.
    export function enrichPlanException(exception: VCRServer.PlanExceptionContract, entryId: string, reload: () => void): IPlanException {
        // Keine sich wiederholende Aufnahme.
        if (!exception)
            return null;

        // Interne Schnittstelle verwenden.
        var enriched = <IPlanExceptionEx>exception;

        // Rohdaten wandeln.
        enriched.exceptionMode = ((enriched.startShift !== 0) || (enriched.timeDelta !== 0)) ? "exceptOn" : "exceptOff";
        enriched.referenceDayDisplay = parseInt(enriched.referenceDayDisplay as string, 10);
        enriched.originalStart = new Date(enriched.originalStart as string);

        // Beachrichtigungen einrichten.
        var site: INoUiSite;

        enriched.setSite = newSite => site = newSite;

        function refresh(): void {
            if (site)
                site.refreshUi();
        }

        // Editierfunktionen anbieten.
        var startSlider = new NumberSlider(enriched, "startShift", refresh, -480, +480);
        var durationSlider = new NumberSlider(enriched, "timeDelta", refresh, -enriched.originalDuration, +480);

        enriched.startSlider = startSlider;
        enriched.durationSlider = durationSlider;

        // Methoden ergänzen.
        function start(): Date {
            return new Date((enriched.originalStart as Date).getTime() + 60 * enriched.startShift * 1000);
        }

        function end(): Date {
            return new Date(start().getTime() + 60 * (enriched.originalDuration + enriched.timeDelta) * 1000);
        }

        enriched.getEnd = () => JMSLib.DateFormatter.getEndTime(end());
        enriched.getStart = () => JMSLib.DateFormatter.getStartTime(start());
        enriched.getDuration = () => enriched.originalDuration + enriched.timeDelta;

        enriched.reset = () => { startSlider.sync(0); durationSlider.sync(0); };
        enriched.disable = () => { startSlider.sync(0); durationSlider.sync(-enriched.originalDuration); };
        enriched.update = () => VCRServer.updateException(entryId, enriched.referenceDay, enriched.startShift, enriched.timeDelta).then(reload);

        // Erweitertes Model melden.
        return enriched;
    }
}