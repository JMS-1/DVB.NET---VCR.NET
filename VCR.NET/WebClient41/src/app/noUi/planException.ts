namespace VCRNETClient.App.NoUi {

    export interface IPlanException extends VCRServer.PlanExceptionContract, INoUiWithSite {
        getStart(): string;
    }

    export function enrichPlanException(exception: VCRServer.PlanExceptionContract): IPlanException {
        if (!exception)
            return null;

        var enriched = <IPlanException>exception;

        enriched.referenceDayDisplay = parseInt(enriched.referenceDayDisplay as string, 10);
        enriched.originalStart = new Date(enriched.originalStart as string);

        enriched.getStart = () => JMSLib.DateFormatter.getStartTime(new Date((enriched.originalStart as Date).getTime() + 60 * enriched.startShift * 1000));

        // Beachrichtigungen einrichten.
        var site: INoUiSite;

        enriched.setSite = newSite => site = newSite;

        // Erweitertes Model melden.
        return enriched;
    }
}