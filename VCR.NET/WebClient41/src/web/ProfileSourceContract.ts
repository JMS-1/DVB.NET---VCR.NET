module VCRServer {

    // Repräsentiert die Klasse ProfileSource
    export interface ProfileSourceContract extends SourceInformationContract {
        // Gesetzt, wenn es sich um einen Fernseh- und nicht einen Radiosender handelt.
        tvNotRadio: boolean;
    }

    export function getProfileSources(device: string): Promise<ProfileSourceContract[]> {
        return doUrlCall(`profile/${device}`);
    }

}

