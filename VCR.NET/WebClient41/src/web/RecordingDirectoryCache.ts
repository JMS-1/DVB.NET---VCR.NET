module VCRServer {

    // Verwaltet die Aufzeichnungsverzeichnisse
    export class RecordingDirectoryCache {
        // Die zwischengespeicherten Verzeichnisse
        private static promise: JMSLib.App.Promise<string[], JMSLib.App.IHttpErrorInformation>;

        // Vergisst alles, was wir wissen
        static reset(): void {
            RecordingDirectoryCache.promise = null;
        }

        // Ruft die Verzeichnisse ab
        static getPromise(): JMSLib.App.IHttpPromise<string[]> {
            // Erstmalig laden
            if (!RecordingDirectoryCache.promise) {
                // Verwaltung erzeugen.
                RecordingDirectoryCache.promise = new JMSLib.App.Promise<string[], JMSLib.App.IHttpErrorInformation>((success, failure) => {
                    getRecordingDirectories().then(data => success(data));
                });
            }

            // Verwaltung melden.
            return RecordingDirectoryCache.promise;
        }
    }

}

