module VCRServer {

    // Repräsentiert die Klasse ProfileInfo
    export interface ProfileInfoContract {
        name: string;
    }

    export function getProfileInfos(): JMSLib.App.IHttpPromise<ProfileInfoContract[]> {
        return doUrlCall(`profile`);
    }

}

