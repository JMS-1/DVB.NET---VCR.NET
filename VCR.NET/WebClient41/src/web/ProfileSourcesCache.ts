module VCRServer {

    // Verwaltet Listen von Quellen zu Geräteprofilen
    export class ProfileSourcesCache {
        // Verwaltet alle Quellen zu allen Geräten als Nachschlageliste
        private static promises: { [device: string]: JMSLib.App.Promise<SourceEntry[], JMSLib.App.IHttpErrorInformation> } = {};

        // Fordert die Quellen eines Geräteprofils an.
        static getSources(profileName: string): JMSLib.App.IHttpPromise<SourceEntry[]> {
            // Eventuell haben wir das schon einmal gemacht
            var promise = ProfileSourcesCache.promises[profileName];
            if (!promise) {
                // Verwaltung erzeugen.
                ProfileSourcesCache.promises[profileName] = promise = new JMSLib.App.Promise<SourceEntry[], JMSLib.App.IHttpErrorInformation>((success, failure) => {
                    // Ladevorgang anstossen.
                    getProfileSources(profileName).then(data => success($.map(data, rawData => new SourceEntry(rawData))));
                });
            }

            // Verwaltung melden.
            return promise;
        }
    }

}

