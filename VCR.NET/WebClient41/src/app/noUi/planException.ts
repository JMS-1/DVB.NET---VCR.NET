namespace VCRNETClient.App.NoUi {

    export interface IPlanException extends VCRServer.PlanExceptionContract, INoUiWithSite {
        readonly startSlider: INumberSlider;

        readonly durationSlider: INumberSlider;

        getStart(): string;

        getEnd(): string;

        getDuration(): number;

        reset(): void;

        disable(): void;
    }

    interface IPlanExceptionEx extends IPlanException {
        startSlider: NumberSlider;

        durationSlider: NumberSlider;
    }

    export function enrichPlanException(exception: VCRServer.PlanExceptionContract): IPlanException {
        if (!exception)
            return null;

        var enriched = <IPlanExceptionEx>exception;

        // Rohdaten wandeln.
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

        // Erweitertes Model melden.
        return enriched;
    }
}