module VCRServer {

    // Repräsentiert die Klasse ProfileInfo
    export interface ProfileInfoContract {
        name: string;
    }

    export function getProfileInfos(): Promise<ProfileInfoContract[]> {
        return doUrlCall(`profile`);
    }

}

