module VCRServer {

    // Repräsentiert die Klasse PlanException
    export interface PlanExceptionContract {
        // Der zugehörige Tag als interner Schlüssel, der unverändert zwischen Client und Service ausgetauscht wird
        referenceDay: string;

        // Der zugehörige Tag repräsentiert Date.getTime() Repräsentation
        referenceDayDisplay: string;

        // Die aktuelle Verschiebung des Startzeitpunktes in Minuten
        startShift: number;

        // Die aktuelle Veränderung der Laufzeit in Minuten
        timeDelta: number;

        // Der ursprüngliche Startzeitpunkt in ISO Notation
        originalStart: string;

        // Die ursprüngliche Dauer in Minuten
        originalDuration: number;
    }

    export function updateException(legacyId: string, referenceDay: string, startDelta: number, durationDelta: number): Promise<void> {
        return doUrlCall<void, void>(`exception/${legacyId}?when=${referenceDay}&startDelta=${startDelta}&durationDelta=${durationDelta}`, `PUT`);
    }

}

