module VCRServer {

     // Verwaltet die Geräteprofile
    export class ProfileCache {
        // Die zwischengespeicherten Geräte
        private static promise: JMSLib.App.Promise<ProfileInfoContract[], JMSLib.App.IHttpErrorInformation>;

        // Ruft die Profile ab
        static getAllProfiles(): JMSLib.App.IHttpPromise<ProfileInfoContract[]> {
            // Einmalig erzeugen.
            if (!ProfileCache.promise) {
                ProfileCache.promise = new JMSLib.App.Promise<ProfileInfoContract[], JMSLib.App.IHttpErrorInformation>((success, failure) => {
                    // Ladevorgang anstossen.
                    getProfileInfos().then(data => success(data));
                });
            }

            // Verwaltung melden.
            return ProfileCache.promise;
        }
    }

}

