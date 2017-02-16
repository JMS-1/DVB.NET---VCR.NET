module VCRServer {

    // Repräsentiert die Klasse ProfileJobInfo
    export interface ProfileJobInfoContract {
        // Der Name des Auftrags
        name: string;

        // Die eindeutige Kennung des Auftrags
        id: string;
    }

    export function getProfileJobInfos(device: string): JMSLib.App.IHttpPromise<ProfileJobInfoContract[]> {
        return doUrlCall(`profile/${device}?activeJobs`);
    }

}

