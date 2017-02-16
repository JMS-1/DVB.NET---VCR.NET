module VCRServer {

    // Verwaltet die Zusammenfassung der Daten der Programmzeitschrift für einzelne Geräte
    export class GuideInfoCache {
        private static promises: { [device: string]: JMSLib.App.Promise<GuideInfoContract, JMSLib.App.IHttpErrorInformation> } = {};

        static getPromise(profileName: string): JMSLib.App.IHttpPromise<GuideInfoContract> {
            // Eventuell haben wir das schon einmal gemacht
            var promise = GuideInfoCache.promises[profileName];
            if (!promise)
                GuideInfoCache.promises[profileName] =
                    promise = new JMSLib.App.Promise<GuideInfoContract, JMSLib.App.IHttpErrorInformation>(success => getGuideInfo(profileName).then(success));

            // Verwaltung melden.
            return promise;
        }
    }

}

