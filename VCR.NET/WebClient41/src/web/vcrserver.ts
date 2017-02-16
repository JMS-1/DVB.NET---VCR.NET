/// <reference path='../lib/http/config.ts' />

module VCRServer {
    // Normalerweise sind wir das selbst
    var serverRoot = document.URL.substr(0, document.URL.indexOf('/', document.URL.indexOf('://') + 3));

    // Schauen wir uns mal die Betriebsart an
    var query = window.location.search;
    if (query == '?debug')
        serverRoot = 'http://localhost:81';

    // Der Präfix für den Zugriff auf Geräte und Dateien
    var protocolEnd = serverRoot.indexOf('://');
    var deviceUrl = 'dvbnet' + serverRoot.substr(protocolEnd) + '/';
    var playUrl = deviceUrl + 'play=';

    // Der Präfix für alle REST Zugiffe
    var restRoot = serverRoot + '/vcr.net/';

    // Bibliothek konfigurieren.
    JMSLib.App.webCallRoot = restRoot;

    function doUrlCall<TResponseType, TRequestType>(url: string, method: string = 'GET', request?: TRequestType): JMSLib.App.IHttpPromise<TResponseType> {
        return JMSLib.App.browserWebCall(url, method, request);
    }    

    export function getRestRoot(): string {
        return restRoot;
    }

    export function getServerRoot(): string {
        return serverRoot;
    }

    export function getFilePlayUrl(path: string): string {
        return `${playUrl}${encodeURIComponent(path)}`;
    }

    export function getDeviceRoot(): string {
        return deviceUrl;
    }

    export function getServerVersion(): JMSLib.App.IHttpPromise<InfoServiceContract> {
        return doUrlCall('info');
    }

    export function getProfileInfos(): JMSLib.App.IHttpPromise<ProfileInfoContract[]> {
        return doUrlCall('profile');
    }

    export function getUserProfile(): JMSLib.App.IHttpPromise<UserProfileContract> {
        return doUrlCall(`userprofile`);
    }

    export function setUserProfile(profile: UserProfileContract): JMSLib.App.IHttpPromise<UserProfileContract> {
        return doUrlCall(`userprofile`, `PUT`, profile);
    }

    export function updateSearchQueries(queries: SavedGuideQueryContract[]): JMSLib.App.IHttpPromise<void> {
        return doUrlCall<void, SavedGuideQueryContract[]>(`userprofile?favorites`, `PUT`, queries);
    }

    export function getPlanCurrent(): JMSLib.App.IHttpPromise<PlanCurrentContract[]> {
        return doUrlCall('plan');
    }

    export function getProfileJobInfos(device: string): JMSLib.App.IHttpPromise<ProfileJobInfoContract[]> {
        return doUrlCall(`profile/${device}?activeJobs`);
    }

    export function getGuideInfo(device: string): JMSLib.App.IHttpPromise<GuideInfoContract> {
        return doUrlCall(`guide/${device}`);
    }

    export function getInfoJobs(): JMSLib.App.IHttpPromise<InfoJobContract[]> {
        return doUrlCall(`info?jobs`);
    }

    export function getProfileSources(device: string): JMSLib.App.IHttpPromise<ProfileSourceContract[]> {
        return doUrlCall(`profile/${device}`);
    }

    export function getSecuritySettings(): JMSLib.App.IHttpPromise<SecuritySettingsContract> {
        return doUrlCall(`configuration?security`);
    }

    export function setSecuritySettings(data: SecuritySettingsContract): JMSLib.App.IHttpPromise<boolean> {
        return doUrlCall(`configuration?security`, `PUT`, data);
    }

    export function getDirectorySettings(): JMSLib.App.IHttpPromise<DirectorySettingsContract> {
        return doUrlCall(`configuration?directory`);
    }

    export function setDirectorySettings(data: DirectorySettingsContract): JMSLib.App.IHttpPromise<boolean> {
        return doUrlCall(`configuration?directory`, `PUT`, data);
    }

    export function getGuideSettings(): JMSLib.App.IHttpPromise<GuideSettingsContract> {
        return doUrlCall(`configuration?guide`);
    }

    export function setGuideSettings(data: GuideSettingsContract): JMSLib.App.IHttpPromise<boolean> {
        return doUrlCall(`configuration?guide`, `PUT`, data);
    }

    export function getSourceScanSettings(): JMSLib.App.IHttpPromise<SourceScanSettingsContract> {
        return doUrlCall(`configuration?scan`);
    }

    export function setSourceScanSettings(data: SourceScanSettingsContract): JMSLib.App.IHttpPromise<boolean> {
        return doUrlCall(`configuration?scan`, `PUT`, data);
    }

    export function getProfileSettings(): JMSLib.App.IHttpPromise<ProfileSettingsContract> {
        return doUrlCall(`configuration?devices`);
    }

    export function setProfileSettings(data: ProfileSettingsContract): JMSLib.App.IHttpPromise<boolean> {
        return doUrlCall(`configuration?devices`, `PUT`, data);
    }

    export function getOtherSettings(): JMSLib.App.IHttpPromise<OtherSettingsContract> {
        return doUrlCall(`configuration?other`);
    }

    export function setOtherSettings(data: OtherSettingsContract): JMSLib.App.IHttpPromise<boolean> {
        return doUrlCall(`configuration?other`, `PUT`, data);
    }

    export function getSchedulerRules(): JMSLib.App.IHttpPromise<SchedulerRulesContract> {
        return doUrlCall(`configuration?rules`);
    }

    export function setSchedulerRules(data: SchedulerRulesContract): JMSLib.App.IHttpPromise<boolean> {
        return doUrlCall(`configuration?rules`, `PUT`, data);
    }

    export function getProtocolEntries(device: string, startDay: Date, endDay: Date): JMSLib.App.IHttpPromise<ProtocolEntryContract[]> {
        return doUrlCall(`protocol/${device}?start=${startDay.toISOString()}&end=${endDay.toISOString()}`);
    }

    export function queryProgramGuide(filter: GuideFilterContract): JMSLib.App.IHttpPromise<GuideItemContract[]> {
        return doUrlCall("guide", `POST`, filter);
    }

    export function countProgramGuide(filter: GuideFilterContract): JMSLib.App.IHttpPromise<number> {
        return doUrlCall(`guide?countOnly`, `POST`, filter);
    }

    export function getRecordingDirectories(): JMSLib.App.IHttpPromise<string[]> {
        return doUrlCall('info?directories');
    }

    export function getGuideItem(device: string, source: string, start: Date, end: Date): JMSLib.App.IHttpPromise<GuideItemContract> {
        return doUrlCall(`guide?profile=${encodeURIComponent(device)}&source=${source}&pattern=${start.getTime()}-${end.getTime()}`);
    }

    export function updateException(legacyId: string, referenceDay: string, startDelta: number, durationDelta: number): JMSLib.App.IHttpPromise<void> {
        return doUrlCall<void, void>(`exception/${legacyId}?when=${referenceDay}&startDelta=${startDelta}&durationDelta=${durationDelta}`, 'PUT');
    }

    export function triggerTask(taskName: string): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'plan?' + taskName,
            dataType: 'json',
            type: 'POST',
        });
    }

    export function browseDirectories(root: string, children: boolean): JMSLib.App.IHttpPromise<string[]> {
        return doUrlCall(`configuration?browse&toParent=${!children}&root=${encodeURIComponent(root)}`);
    }

    export function deleteSchedule(jobId: string, scheduleId: string): JMSLib.App.IHttpPromise<void> {
        return doUrlCall<void, void>(`edit/${jobId}${scheduleId}`, `DELETE`);
    }

    export function updateEndTime(device: string, suppressHibernate: boolean, scheduleIdentifier: string, newEnd: Date): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'profile/' + device + '?disableHibernate=' + suppressHibernate + '&schedule=' + scheduleIdentifier + '&endTime=' + newEnd.toISOString(),
            dataType: 'json',
            type: 'PUT',
        });
    }

    export function getWindowsGroups(): JMSLib.App.IHttpPromise<string[]> {
        return doUrlCall(`info?groups`);
    }

    export function updateConfiguration(type: string, contract: SettingsContract, protocolFilter: (key: string, value: any) => any = null): JQueryPromise<any> {
        if (protocolFilter == null)
            protocolFilter = (key: string, value: any) => value;

        return $.ajax({
            data: JSON.stringify(contract, protocolFilter),
            url: restRoot + 'configuration?' + type,
            contentType: 'application/json',
            type: 'PUT',
        });
    }

    export function validateDirectory(path: string): JMSLib.App.IHttpPromise<boolean> {
        return doUrlCall(`configuration?validate&directory=${encodeURIComponent(path)}`);
    }

    export function createScheduleFromGuide(legacyId: string, epgId: string): JMSLib.App.IHttpPromise<JobScheduleInfoContract> {
        return doUrlCall(`edit/${legacyId}?epg=${epgId}`);
    }

    export function getPlan(limit: number, end: Date): JMSLib.App.IHttpPromise<PlanActivityContract[]> {
        return doUrlCall(`plan?limit=${limit}&end=${end.toISOString()}`);
    }

    export function updateSchedule(jobId: string, scheduleId: string, data: JobScheduleDataContract): JMSLib.App.IHttpPromise<void> {
        var method = "POST";
        var url = "edit";

        if (jobId != null) {
            url += '/' + jobId;

            if (scheduleId != null) {
                url += scheduleId;

                method = 'PUT';
            }
        }

        // Befehl ausführen
        return doUrlCall<void, JobScheduleDataContract>(url, method, data);
    }

    // Beschreibt die Daten der Programmzeitschrift für ein Gerät
    export class GuideInfo {
        constructor(rawData: GuideInfoContract) {
            this.firstStart = (rawData.first == null) ? null : new Date(rawData.first);
            this.lastStart = (rawData.last == null) ? null : new Date(rawData.last);
            this.stations = rawData.stations;
        }

        // Der Zeitpunkt, an dem die früheste Sendung beginnt
        firstStart: Date;

        // Der Zeitpunkt, an dem die späteste Sendung beginnt
        lastStart: Date;

        // Alle Quellen, zu denen Einträge in der Programmzeitschrift existieren
        stations: string[];
    }

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
        firstNameCharacter: string;

        // Gesetzt, wenn es sich um einen Fernsehsender handelt.
        isTelevision: boolean;

        // Gesetzt, wenn die Quelle verschlüsselt sendet.
        isEncrypted: boolean;

        // Der eindeutige Name der Quelle.
        name: string;
    }

    // Beschreibt die individuellen Einstellungen des Anwenders
    export class UserProfile {
        // Die Anzahl der Vorschautage im Aufzeichnungsplan
        planDaysToShow: number = 0;

        // Gesetzt, wenn die Liste der bisher verwendeten Quellen nicht leer ist
        hasRecentSources: boolean = false;

        // Alle bisher verwendeten Quellen als Nachschlageliste
        recentSources: any = {};

        // Die bevorzugte Auswahl der Art der Quellen (R=Radio, T=Fernsehen)
        defaultType: string = 'RT';

        // Die bevorzugte Auswahl der Verschlüsselung der Quellen (F=unverschlüsselt, P=verschlüsselt)
        defaultEncryption: string = 'FP';

        // Gesetzt, wenn bevorzugt alle Sprachen aufgezeichnet werden sollen
        defaultAllLanguages: boolean = false;

        // Gesetzt, wenn bevorzugt die Dolby Digital Tonspur mit aufgezeichnet werden soll
        defaultDolby: boolean = false;

        // Gesetzt, wenn bevorzugt der Videotext mit aufgezeichnet werden soll
        defaultVideotext: boolean = false;

        // Gesetzt, wenn bevorzugt DVB Untertitel mit aufgezeichnet werden sollen
        defaultDVBSubtitles: boolean = false;

        // Gesetzt, wenn nach dem Anlegen einer Aufzeichnung aus der Programmzeitschrift diese wieder aktiviert werden soll
        guideAfterAdd: boolean = false;

        // Die Anzahl der Sendungen pro Seite der Programmzeitschrift
        rowsInGuide: number = 30;

        // Die Vorlaufzeit (in Minuten) von Aufzeichnungen bei der Programmierung über die Programmzeitschrift
        guidePreTime: number = 15;

        // Die Nachlaufzeit (in Minuten) von Aufzeichnungen bei der Programmierung über die Programmzeitschrift
        guidePostTime: number = 30;

        // Die maximale Anzahl von Quellen in der Liste der zuletzt verwendeten Quellen
        maximumRecentSources: number = 10;

        // Gesetzt, wenn bevorzugt beim Abbruch einer laufenden Aufzeichnung oder Aufgabe der Übergang in den Schlafzustand deaktiviert werden soll
        noHibernateOnAbort: boolean = false;

        // Die gespeicherten Suchen der Programmzeitschrift
        guideSearches: string = null;

        // Wird gesetzt, sobald Daten zur Verfügung stehen
        private isLoaded: boolean = false;

        // Wird aufgerufen, sobald Daten zur Verfügung stehen
        private loaded: () => void = null;

        // Meldet Interesse am Ladezustand an
        register(whenLoaded: () => void): void {
            this.loaded = whenLoaded;

            if (this.isLoaded)
                if (whenLoaded != null)
                    whenLoaded();
        }

        // Ruft eine aktuelle Konfiguration vom Web Service ab.
        refresh(): void {
            this.isLoaded = false;

            getUserProfile().then(data => this.loadFrom(data));
        }

        // Übernimmt neue Daten.
        private loadFrom(data: UserProfileContract): void {
            this.isLoaded = true;

            // Liste der Quelle in eine Nachschlagekarte umsetzen
            this.recentSources = {};
            $.each(data.recentSources, (index: number, sourceName: string) => this.recentSources[sourceName] = true);

            // Das meiste wird unverändert übernommen
            this.hasRecentSources = data.recentSources.length > 0;
            this.maximumRecentSources = data.recentSourceLimit;
            this.noHibernateOnAbort = data.suppressHibernate;
            this.defaultEncryption = data.encryptionFilter;
            this.defaultAllLanguages = data.languages;
            this.defaultDVBSubtitles = data.subtitles;
            this.guidePreTime = data.guideAheadStart;
            this.guidePostTime = data.guideBeyondEnd;
            this.guideSearches = data.guideSearches;
            this.defaultVideotext = data.videotext;
            this.guideAfterAdd = data.backToGuide;
            this.planDaysToShow = data.planDays;
            this.defaultType = data.typeFilter;
            this.rowsInGuide = data.guideRows;
            this.defaultDolby = data.dolby;

            // Benachrichtigung auslösen
            if (this.loaded != null)
                this.loaded();
        }

        // Erstellt die Repräsentation, die auch an den Server geschickt werden kann.
        private toContract(): UserProfileContract {
            var profileData: UserProfileContract =
                {
                    recentSourceLimit: this.maximumRecentSources,
                    suppressHibernate: this.noHibernateOnAbort,
                    encryptionFilter: this.defaultEncryption,
                    languages: this.defaultAllLanguages,
                    subtitles: this.defaultDVBSubtitles,
                    guideAheadStart: this.guidePreTime,
                    guideBeyondEnd: this.guidePostTime,
                    guideSearches: this.guideSearches,
                    videotext: this.defaultVideotext,
                    backToGuide: this.guideAfterAdd,
                    planDays: this.planDaysToShow,
                    typeFilter: this.defaultType,
                    guideRows: this.rowsInGuide,
                    recentSources: new Array(),
                    dolby: this.defaultDolby,
                };

            return profileData;
        }

        // Erstellt eine fast exakte Kopie - die Liste der zuletzt verwendeten Sourcen wird nicht benötigt.
        clone(): UserProfile {
            var profile = new UserProfile();

            profile.loadFrom(this.toContract());

            return profile;
        }

        // Sendet die aktuelle Konfiguration an den Web Service.
        update(onError: (message: string) => void): void {
            setUserProfile(this.toContract())
                .then(data => { UserProfile.global.loadFrom(data); window.location.hash = 'home'; }, JMSLib.dispatchErrorMessage(onError));
        }

        // Die einzige Instanz der Einstellungen.
        static global = new UserProfile();
    }

}

