module VCRServer {

    // Repräsentiert die Klasse SourceInformation
    export interface SourceInformationContract {
        // Der volle Name der Quelle
        nameWithProvider: string;

        // Gesetzt, wenn die Quelle verschlüsselt ist
        encrypted: boolean;
    }

}

