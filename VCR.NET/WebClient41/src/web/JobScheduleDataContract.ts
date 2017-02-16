module VCRServer {

    // Repräsentiert die Klasse JobScheduleData
    export interface JobScheduleDataContract {
        // Der Auftrag
        job: EditJobContract;

        // Die Aufzeichnung im Auftrag
        schedule: EditScheduleContract;
    }

}

