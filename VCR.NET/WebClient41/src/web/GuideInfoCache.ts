module VCRServer {

    // Verwaltet die Zusammenfassung der Daten der Programmzeitschrift für einzelne Geräte
    export class GuideInfoCache {
        private static promises: { [device: string]: Promise<GuideInfoContract> } = {};

        static getPromise(profileName: string): Promise<GuideInfoContract> {
            // Eventuell haben wir das schon einmal gemacht
            var promise = GuideInfoCache.promises[profileName];
            if (!promise)
                GuideInfoCache.promises[profileName] =
                    promise = new Promise<GuideInfoContract>(success => getGuideInfo(profileName).then(success));

            // Verwaltung melden.
            return promise;
        }
    }

}

