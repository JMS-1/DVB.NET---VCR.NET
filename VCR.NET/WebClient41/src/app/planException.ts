namespace VCRNETClient.App {

    export interface IPlanException extends VCRServer.PlanExceptionContract {
    }

    export function enrichPlanException(exception: VCRServer.PlanExceptionContract): IPlanException {
        if (!exception)
            return null;

        // Das Model vom Server wird nur gekapselt und ist unveränderlich - eine JSON Serialisierung ist so allerdings nicht mehr möglich.
        var enriched = <IPlanException>{ ["__proto__"]: exception };

        enriched.referenceDayDisplay = parseInt(<string>enriched.referenceDayDisplay, 10);
        enriched.originalStart = new Date(<string>enriched.originalStart);

        return enriched;
    }
}