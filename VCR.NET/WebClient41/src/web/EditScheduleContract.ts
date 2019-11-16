module VCRServer {

    // Repräsentiert die Klasse EditSchedule
    export interface EditScheduleContract extends EditJobScheduleCommonContract {
        // Der erste Startzeitpunkt in ISO Notation
        firstStart: string;

        // Die Wochentage, an denen eine Wiederholung stattfinden soll
        repeatPattern: number;

        // Der Tag, an dem die letzte Aufzeichnung stattfinden soll
        lastDay: string;

        // Die Dauer in Minuten
        duration: number;

        // Die Liste der Ausnahmeregeln
        exceptions: PlanExceptionContract[];
    }

    export function deleteSchedule(jobId: string, scheduleId: string): Promise<void> {
        return doUrlCall<void, void>(`edit/${jobId}${scheduleId}`, `DELETE`);
    }

}

