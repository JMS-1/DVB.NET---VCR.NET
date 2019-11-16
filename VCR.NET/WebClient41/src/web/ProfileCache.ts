module VCRServer {

     // Verwaltet die Geräteprofile
    export class ProfileCache {
        // Die zwischengespeicherten Geräte
        private static promise: Promise<ProfileInfoContract[]>;

        // Ruft die Profile ab
        static getAllProfiles(): Promise<ProfileInfoContract[]> {
            // Einmalig erzeugen.
            if (!ProfileCache.promise) {
                ProfileCache.promise = new Promise<ProfileInfoContract[]>((success, failure) => {
                    // Ladevorgang anstossen.
                    getProfileInfos().then(data => success(data));
                });
            }

            // Verwaltung melden.
            return ProfileCache.promise;
        }
    }

}

