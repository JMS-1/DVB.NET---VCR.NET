module VCRServer {

    // Verwaltet die Aufzeichnungsverzeichnisse
    export class RecordingDirectoryCache {
        // Die zwischengespeicherten Verzeichnisse
        private static promise: Promise<string[]>;

        // Vergisst alles, was wir wissen
        static reset(): void {
            RecordingDirectoryCache.promise = null;
        }

        // Ruft die Verzeichnisse ab
        static getPromise(): Promise<string[]> {
            // Erstmalig laden
            if (!RecordingDirectoryCache.promise) {
                // Verwaltung erzeugen.
                RecordingDirectoryCache.promise = new Promise<string[]>((success, failure) => {
                    getRecordingDirectories().then(data => success(data));
                });
            }

            // Verwaltung melden.
            return RecordingDirectoryCache.promise;
        }
    }

}

