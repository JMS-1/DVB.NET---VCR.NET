module VCRServer {

    // Repräsentiert die Klasse JobScheduleData
    export interface JobScheduleDataContract {
        // Der Auftrag
        job: EditJobContract;

        // Die Aufzeichnung im Auftrag
        schedule: EditScheduleContract;
    }

    export function createScheduleFromGuide(legacyId: string, epgId: string): Promise<JobScheduleInfoContract> {
        return doUrlCall(`edit/${legacyId}?epg=${epgId}`);
    }

    export function updateSchedule(jobId: string, scheduleId: string, data: JobScheduleDataContract): Promise<void> {
        var method = `POST`;
        var url = `edit`;

        if (jobId != null) {
            url += `/` + jobId;

            if (scheduleId != null) {
                url += scheduleId;

                method = `PUT`;
            }
        }

        // Befehl ausführen
        return doUrlCall<void, JobScheduleDataContract>(url, method, data);
    }

}

