module VCRServer {

    // Repräsentiert die Klasse JobScheduleInfo
    export interface JobScheduleInfoContract {
        // Der Auftrag
        job: EditJobContract;

        // Die Aufzeichnung im Auftrag
        schedule: EditScheduleContract;

        // Optional die eindeutige Kennung des Auftrags
        jobId: string;

        // Optional die eindeutige Kennung der Aufzeichnung
        scheduleId: string;
    }

}

