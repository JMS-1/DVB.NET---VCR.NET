namespace VCRNETClient.App {

    // Beschreibt die Daten eines Auftrags
    export class JobData {
        constructor(existingData: VCRServer.JobScheduleInfoContract, defaultProfile: string) {
            // Schauen wir mal, ob wir etwas ändern sollen
            if (existingData != null) {
                // Auftragsdaten müssen vorhanden sein
                var rawData = existingData.job;
                if (rawData != null) {
                    // Da gibt es schon etwas für uns vorbereitetes
                    this.lockedToDevice = rawData.lockedToDevice;
                    this.withSubtitles = rawData.withSubtitles;
                    this.withVideotext = rawData.withVideotext;
                    this.allLanguages = rawData.allLanguages;
                    this.includeDolby = rawData.includeDolby;
                    this.sourceName = rawData.sourceName;
                    this.directory = rawData.directory;
                    this.id = existingData.jobId;
                    this.device = rawData.device;
                    this.name = rawData.name;

                    return;
                }
            }

            // Ein ganz neuer Auftrag
            this.withSubtitles = VCRServer.UserProfile.global.defaultDVBSubtitles;
            this.allLanguages = VCRServer.UserProfile.global.defaultAllLanguages;
            this.withVideotext = VCRServer.UserProfile.global.defaultVideotext;
            this.includeDolby = VCRServer.UserProfile.global.defaultDolby;
            this.device = defaultProfile;
            this.lockedToDevice = false;
            this.sourceName = '';
            this.directory = '';
            this.name = '';
            this.id = null;
        }

        // Die Kennung des Auftrags - leer bei neuen Aufträgen.
        id: string;

        // Der Name des Auftrags.
        name: string;

        // Das Aufzeichnungsverzeichnis.
        directory: string;

        // Das zu verwendende DVB Gerät.
        device: string;

        // Gesetzt, wenn die Aufzeichnung immer auf dem Gerät stattfinden soll.
        lockedToDevice: boolean;

        // Gesetzt, wenn alle Sprachen aufgezeichnet werden sollen.
        allLanguages: boolean;

        // Gesetzt, wenn auch die AC3 Tonspur aufgezeichnet werden soll.
        includeDolby: boolean;

        // Gesetzt, wenn auch der Videotext aufgezeichnet werden soll.
        withVideotext: boolean;

        // Gesetzt, wenn auch DVB Untertitel aufgezeichnet werden sollen.
        withSubtitles: boolean;

        // Der Name der Quelle, die aufgezeichnet werden soll.
        sourceName: string;

        // Erstellt eine für die Datenübertragung geeignete Variante.
        toWebService(): VCRServer.EditJobContract {
            var contract: VCRServer.EditJobContract = {
                lockedToDevice: this.lockedToDevice,
                withVideotext: this.withVideotext,
                withSubtitles: this.withSubtitles,
                allLanguages: this.allLanguages,
                includeDolby: this.includeDolby,
                sourceName: this.sourceName,
                directory: this.directory,
                device: this.device,
                name: this.name
            };

            return contract;
        }
    }

}