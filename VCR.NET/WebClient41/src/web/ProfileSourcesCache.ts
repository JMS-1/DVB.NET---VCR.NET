module VCRServer {

    // Beschreibt einen einzelne Quelle, so wie sie dem Anwender zur Auswahl angeboten wird
    export class SourceEntry {
        constructor(rawData: ProfileSourceContract) {
            this.isTelevision = rawData.tvNotRadio;
            this.name = rawData.nameWithProvider;
            this.isEncrypted = rawData.encrypted;

            // Zum schnellen Auswählen nach dem Namen
            this.firstNameCharacter = this.name.toUpperCase().charAt(0);
        }

        // Zur schnellen Auswahl das erste Zeichen des Namens der Quelle in Großschreibung.
        readonly firstNameCharacter: string;

        // Gesetzt, wenn es sich um einen Fernsehsender handelt.
        readonly isTelevision: boolean;

        // Gesetzt, wenn die Quelle verschlüsselt sendet.
        readonly isEncrypted: boolean;

        // Der eindeutige Name der Quelle.
        readonly name: string;
    }

    // Verwaltet Listen von Quellen zu Geräteprofilen
    export class ProfileSourcesCache {
        // Verwaltet alle Quellen zu allen Geräten als Nachschlageliste
        private static promises: { [device: string]: Promise<SourceEntry[]> } = {};

        // Fordert die Quellen eines Geräteprofils an.
        static getSources(profileName: string): Promise<SourceEntry[]> {
            // Eventuell haben wir das schon einmal gemacht
            var promise = ProfileSourcesCache.promises[profileName];
            if (!promise) {
                // Verwaltung erzeugen.
                ProfileSourcesCache.promises[profileName] = promise = new Promise<SourceEntry[]>((success, failure) => {
                    // Ladevorgang anstossen.
                    getProfileSources(profileName).then(data => success(data.map(rawData => new SourceEntry(rawData))));
                });
            }

            // Verwaltung melden.
            return promise;
        }
    }

}

