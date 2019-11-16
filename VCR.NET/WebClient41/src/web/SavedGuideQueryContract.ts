module VCRServer {

    export interface SavedGuideQueryContract {
        // Das zu berücksichtigende Gerät
        device: string;

        // Optional die Quelle
        source: string;

        // Der Text zur Suche in der üblichen Notation mit der Suchart als erstem Zeichen
        text: string;

        // Gesetzt, wenn nur im Titel gesucht werden soll
        titleOnly: boolean;

        // Die Art der zu berücksichtigenden Quelle
        sourceType: VCRServer.GuideSource;

        // Die Art der Verschlüsselung
        encryption: VCRServer.GuideEncryption;
    }

    export function updateSearchQueries(queries: SavedGuideQueryContract[]): Promise<void> {
        return doUrlCall<void, SavedGuideQueryContract[]>(`userprofile?favorites`, `PUT`, queries);
    }

}

