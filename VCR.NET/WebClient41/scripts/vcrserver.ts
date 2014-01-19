/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='jmslib.ts' />

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

    // Repräsentiert die Klasse InfoService
    export interface InfoServiceContract {
        // Die aktuelle Version des Dienstes in der Notation MAJOR.MINOR [DD.MM.YYYY]
        version: string;

        // Die aktuelle Version der Installation in der Notation MAJOR.MINOR.BUILD
        msiVersion: string;

        // Gesetzt, wenn mindestens ein Gerät eine Aufzeichnung oder Aufgabe ausführt
        active: boolean;

        // Gesetzt, wenn der Anwender ein Administrator ist
        userIsAdmin: boolean;

        // Gesetzt, wenn die Aktualisierung der Quellen verfügbar ist
        canScan: boolean;

        // Gesetzt, wenn die Aktualisierung der Programmzeitschrift verfügbar ist
        hasGuides: boolean;
    }

    // Repräsentiert die Klasse ProfileInfo
    export interface ProfileInfoContract {
        name: string;
    }

    // Repräsentiert die Klasse UserProfile
    export interface UserProfileContract {
        // Die Anzahl der Einträge im Aufzeichnungsplan
        planDays: number;

        // Die Liste der zuletzt verwendeten Quellen
        recentSources: string[];

        // Die Art der Quelle
        typeFilter: string;

        // Die Art der Verschlüsselung
        encryptionFilter: string;

        // Gesetzt, wenn alle Sprachen aufgezeichnet werden sollen
        languages: boolean;

        // Gesetzt, wenn die Dolby Digital Tonspur aufgezeichnet werden soll
        dolby: boolean;

        // Gesetzt, wenn der Videotext aufgezeichnet werden soll
        videotext: boolean;

        // Gesetzt, wenn DVB Untertitel aufgezeichnet werden sollen
        subtitles: boolean;

        // Gesetzt, wenn bei der Programmierung aus der Programmzeitschrift heraus danach in diese zurück gekehrt werden soll
        backToGuide: boolean;

        // Die Anzahl der Sendungen auf einer Seite der Programmzeitschrift
        guideRows: number;

        // Die Vorlaufzeit bei Programmierung aus der Programmzeitschrift heraus
        guideAheadStart: number;

        // Die Nachlaufzeit bei Programmierung aus der Programmzeitschrift heraus
        guideBeyondEnd: number;

        // Die maximal erlaubte Anzahl von zuletzt verwendeten Quellen
        recentSourceLimit: number;

        // Gesetzt, wenn beim Abbruch einer Aufzeichnung der Übergang in den Schlafzustand deaktiviert werden soll
        suppressHibernate: boolean;

        // Die gespeicherten Suchen der Programmzeitschrift
        guideSearches: string;
    }

    // Repräsentiert die Klasse PlanCurrent
    export interface PlanCurrentContractMobile {
        // Das Gerät, auf dem die Aktivität stattfindet
        device: string;

        // Der Name der Aktivität
        name: string;

        // Die Kennung der Quelle
        source: string;

        // Der Name der Quelle
        sourceName: string;

        // Der Startzeitpunkt in ISO Notation
        start: string;

        // Die Dauer in Sekunden
        duration: number;

        // Gesetzt, wenn Daten aus der Programmzeitschrift für die Dauer der Aktivität vorliegen
        epg: boolean;
    };

    export interface PlanCurrentContract extends PlanCurrentContractMobile {
        // Eine eindeutige Kennung einer Aufzeichnung zum Abruf der Detailinformationen
        id: string;

        // Eine eindeutige Kennung einer laufenden Aufzeichnung oder Aufgabe, mit Hilfe derer diese beendet werden kann
        referenceId: string;

        // Gesetzt, wenn eine zukünftige Aktivität verspätet beginnen wird
        late: boolean;

        // Zeigt an, dass dieser Eintrag nur ein Platzhalter für ein Gerät ist, für das keine Planungsdaten vorliegen.
        isIdle: boolean;

        // Hinweistext mit einer Größenangabe
        size: string;

        // Die interne laufende Nummer des Aufzeichnungsdatenstroms
        streamIndex: number;

        // Optional die TCP/IP Adresse, an die gerade ein Netzwerkversand stattfindet
        streamTarget: string;

        // Die verbleibende Anzahl von Minuten einer aktiven Aufzeichnung oder Aufgabe
        remainingMinutes: number;
    }

    // Repräsentiert die Klasse GuideItem
    export interface GuideItemContract {
        // Der Startzeitpunkt in ISO Notation
        start: string;

        // Die Dauer in Sekunden
        duration: number;

        // Der Name der Sendung
        name: string;

        // Die Sprache, in der die Sendung ausgestrahlt wird
        language: string;

        // Die Quelle
        station: string;

        // Die Liste der Alterfreigaben
        ratings: string[];

        // Die Liste der Kategorien
        categories: string[];

        // Die ausführliche Beschreibung
        description: string;

        // Die Kurzbeschreibung
        shortDescription: string;

        // Gesetzt, wenn das Ende der Sendung in der Zukunft liegt
        active: boolean;

        // Die eindeutige Kennung der Sendung
        id: string;
    }

    // Repräsentiert die Klasse GuideInfo
    export interface GuideInfoContract {
        // Alle Quellen eines Gerätes, für das Einträge in der Programmzeitschrift zur Verfügung stehen
        stations: string[];

        // Der Zeitpunkt in ISO Notation, an dem die früheste Sendung beginnt
        first: string;

        // Der Zeitpunkt in ISO Notation, an dem die späteste Sendung beginnt
        last: string;
    }

    // Repräsentiert die Klasse ProfileJobInfo
    export interface ProfileJobInfoContract {
        // Der Name des Auftrags
        name: string;

        // Die eindeutige Kennung des Auftrags
        id: string;
    }

    // Repräsentiert die Klasse PlanException
    export interface PlanExceptionContract {
        // Der zugehörige Tag als interner Schlüssel, der unverändert zwischen Client und Service ausgetauscht wird
        referenceDay: string;

        // Der zugehörige Tag repräsentiert Date.getTime() Repräsentation
        referenceDayDisplay: string;

        // Die aktuelle Verschiebung des Startzeitpunktes in Minuten
        startShift: number;

        // Die aktuelle Veränderung der Laufzeit in Minuten
        timeDelta: number;

        // Der ursprüngliche Startzeitpunkt in ISO Notation
        originalStart: string;

        // Die ursprüngliche Dauer in Minuten
        originalDuration: number;
    }

    // Repräsentiert die Klasse EditSchedule
    export interface EditScheduleContract {
        // Der Name der Aufzeichnung
        name: string;

        // Die verwendete Quelle
        sourceName: string;

        // Gesetzt, wenn alle Sprachen aufgezeichnet werden sollen
        allLanguages: boolean;

        // Gesetzt, wenn die Dolby Digital Tonspur aufgezeichnet werden soll
        includeDolby: boolean;

        // Gesetzt, wenn der Videotext aufgezeichnet werden soll
        withVideotext: boolean;

        // Gesetzt, wenn Untertitel aufgezeichnet werden sollen
        withSubtitles: boolean;

        // Der erste Startzeitpunkt in ISO Notation
        firstStart: string;

        // Die Wochentage, an denen eine Wiederholung stattfinden soll
        repeatPattern: number;

        // Der Tag, an dem die letzte Aufzeichnung stattfinden soll
        lastDay: string;

        // Die Dauer in Minuten
        duration: number;

        // Die Liste der Ausnahmeregeln
        exceptions: PlanExceptionContract[];
    }

    // Repräsentiert die Klasse EditJob
    export interface EditJobContract {
        // Der Name des Auftrags
        name: string;

        // Das zu verwendende Aufzeichnungsverzeichnis
        directory: string;

        // Das zu verwendende Gerät
        device: string;

        // Die Quelle, die für alle Aufzeichnungen verwendet werden soll, die nicht selbst eine solche definieren
        sourceName: string;

        // Gesetzt, wenn die Aufzeichnung auf jeden Fall auf dem angegebenen Geräte erfolgen soll
        lockedToDevice: boolean;

        // Gesetzt um alle Sprachen aufzuzeichnen
        allLanguages: boolean;

        // Gesetzt, um die Dolby Digital Tonspur aufzuzeichnen
        includeDolby: boolean;

        // Gesetzt, um den Videotext aufzuzeichnen
        withVideotext: boolean;

        // Gesetzt, um die Untertitel aufzuzeichnen
        withSubtitles: boolean;
    }

    // Repräsentiert die Klasse JobScheduleInfo
    export interface JobScheduleInfoContract {
        // Der Auftrag
        job: EditJobContract;

        // Die Aufzeichnung im Auftrag
        schedule: EditScheduleContract;

        // Optional die eindeutige Kennung des Auftrags
        jobId: string;

        // Optional die eindeutige Kennung der Aufzeichnung
        scheduleId: string;
    }

    // Repräsentiert die Klasse InfoSchedule
    export interface InfoScheduleContract {
        // Der Name der Aufzeichnung
        name: string;

        // Der erste Start der Aufzeichnung in ISO Notation
        start: string;

        // Die Wochentage, an denen die Aufzeichnugn wiederholt werden soll
        repeatPattern: number;

        // Der Name der Quelle
        sourceName: string;

        // Die eindeutige Kennung der Aufzeichnung
        id: string;
    }

    // Repräsentiert die Klasse InfoJob
    export interface InfoJobContract {
        // Der Name des Auftrags
        name: string;

        // Die eindeutige Kennung des Auftrags
        id: string;

        // Alle Aufzeichnung zum Auftrag
        schedules: InfoScheduleContract[];

        // Gesetzt, wenn der Auftrag noch nicht in das Archiv übertragen wurde
        active: boolean;
    }

    // Repräsentiert die Klasse SourceInformation
    export interface SourceInformationContract {
        // Der volle Name der Quelle
        nameWithProvider: string;

        // Gesetzt, wenn die Quelle verschlüsselt ist
        encrypted: boolean;
    }

    // Repräsentiert die Klasse ProfileSource
    export interface ProfileSourceContract extends SourceInformationContract {
        // Gesetzt, wenn es sich um einen Fernseh- und nicht einen Radiosender handelt.
        tvNotRadio: boolean;
    }

    // Hilfsschnittstelle für alle Einstellungen
    export interface SettingsContract {
    }

    // Repräsentiert die Klasse SecuritySettings
    export interface SecuritySettingsContract extends SettingsContract {
        // Die Windows Gruppe der normalen Benutzer
        users: string;

        // Die Windows Gruppe der Administratoren
        admins: string;
    }

    // Repräsentiert die Klasse DirectorySettings
    export interface DirectorySettingsContract extends SettingsContract {
        // Alle Aufzeichnungsverzeichnisse
        directories: string[];

        // Das Muster für die Erzeugung von Dateinamen
        pattern: string;
    }

    // Repräsentiert die Klasse GuideSettings
    export interface GuideSettingsContract extends SettingsContract {
        // Das Zeitintervall (in Stunden) für vorgezogene Aktualisierungen
        joinHours: string;

        // Das minimale Intervall (in Stunden) zwischen Aktualisierungen
        minDelay: string;

        // Die maximale Dauer einer Aktualisierung (in Minuten)
        duration: number;

        // Die vollen Stunden, zu denen eine Aktualisierung stattfinden soll
        hours: number[];

        // Alle Quellen, die bei der Aktualisierung mit berücksichtigt werden sollen
        sources: string[];

        // Gesetzt, um auch die britische Programmzeitschrift auszuwerten
        includeUK: boolean;
    }

    // Repräsentiert die Klasse SourceScanSettings
    export interface SourceScanSettingsContract extends SettingsContract {
        // Das Zeitinterval (in Stunden) für vorgezogene Aktualisierungen
        joinDays: string;

        // Das minimale Intervall (in Tagen) zwischen den Aktualisierungen - negative Werte für eine ausschließlich manuelle Aktualisierung
        interval: number;

        // Die maximale Dauer einer Aktualisierung (in Minuten)
        duration: number;

        // Die vollen Stunden, zu denen eine Aktualisierung stattfinden soll
        hours: number[];

        // Gesetzt, wenn die neu ermittelten Listen mit den alten zusammengeführt werden sollen
        merge: boolean;
    }

    // Repräsentiert die Klasse ConfigurationProfile
    export interface ProfileContract {
        // Der Name des Gerätes
        name: string;

        // Gesetzt, wenn es für Aufzeichnungen verwendet werden darf
        active: boolean;

        // Die maximale Anzahl gleichzeitig empfangener Quellen
        sourceLimit: number;

        // Die maximale Anzahl gleichzeitig entschlüsselbarer Quellen
        ciLimit: number;

        // Die Aufzeichnungspriorität
        priority: number;
    }

    // Repräsentiert die Klasse ProfileSettings
    export interface ProfileSettingsContract extends SettingsContract {
        // Alle DVB.NET Geräte auf dem Rechner, auf dem der VCR.NET Recording Service läuft
        profiles: ProfileContract[];

        // Das bevorzugte Gerät für neue Aufzeichnungen
        defaultProfile: string;
    }

    // Repräsentiert die Klasse OtherSettings
    export interface OtherSettingsContract extends SettingsContract {
        // Gesetzt, um den Übergang in den Schlafzustand erlauben
        mayHibernate: boolean;

        // Gesetzt, um StandBy für den Schlafzustand zu verwenden
        useStandBy: boolean;

        // Die Verweildauer von Aufträgen im Archiv (in Wochen)
        archive: number;

        // Die Verweildauer von Protokollen (in Wochen)
        protocol: number;

        // Die Vorlaufzeit beim Aufwecken aus dem Schlafzustand (in Sekunden)
        hibernationDelay: number;

        // Gesetzt, um eine PCR Erzeugunbg bei H.264 Material zu vermeiden
        noH264PCR: boolean;

        // Gesetzt, um eine PCR Erzeugung aus MPEG-2 Material zu vermeiden
        noMPEG2PCR: boolean;

        // Die minimale Verweildauer im Schlafzustand
        forcedHibernationDelay: number;

        // Gesetzt, wenn die minimale Verweildauer im Schlafzustand ignoriert werden soll
        suppressHibernationDelay: boolean;

        // Gesetzt, um auch das Basic Protokoll zur Autentisierung zu erlauben
        basicAuth: boolean;

        // Gesetzt, um die Verbindung zu verschlüsseln
        ssl: boolean;

        // Der TCP/IP Port für verschlüsselte Verbindungen
        sslPort: number;

        // Der TCP/IP Port für reguläre Anfragen
        webPort: number;

        // Die Art der Protokollierung
        logging: string;
    }

    // Repräsentiert die Klasse SchedulerRules
    export interface SchedulerRulesContract extends SettingsContract {
        // Die Liste der Regeln
        rules: string;
    }

    // Repräsentiert die Klasse ProtocolEntry
    export interface ProtocolEntryContract {
        // Der Startzeitpunkt in ISO Notation
        start: string;

        // Der Endzeitpunkt in ISO Notation
        end: string;

        // Der Name der zuerst verwendeten Quelle
        firstSourceName: string;

        // Der Name der primären Aufzeichnungsdatei
        primaryFile: string;

        // Die Liste der erzeugten Dateien
        files: string[];

        // Ein Hinweis zur Anzeige der Größe
        size: string;
    }

    // Repräsentiert die Klasse GuideFilter
    export interface GuideFilterContract {
        // Der Name des aktuell ausgewählten Geräteprofils
        device: string;

        // Der Name der aktuell ausgewählten Quelle
        station: string;

        // Der minimale Startzeitpunkt in ISO Notation
        start: string;

        // Das Suchmuster für den Namen einer Sendung
        title: string;

        // Das Suchmuster für die Beschreibung einer Sendung
        content: string;

        // Die Anzahl von Sendungen pro Anzeigeseite
        size: number;

        // Die aktuelle Seite
        index: number;
    }

    // Repräsentiert die Klasse JobScheduleData
    export interface JobScheduleDataContract {
        // Der Auftrag
        job: EditJobContract;

        // Die Aufzeichnung im Auftrag
        schedule: EditScheduleContract;
    }

    export function getRestRoot(): string {
        return restRoot;
    }

    export function getServerRoot(): string {
        return serverRoot;
    }

    export function getFileRoot(): string {
        return playUrl;
    }

    export function getDeviceRoot(): string {
        return deviceUrl;
    }

    export function getServerVersion(): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'info',
            dataType: 'json',
        });
    }

    export function getProfileInfos(): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'profile',
            dataType: 'json',
        });
    }

    export function getUserProfile(): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'userprofile',
            dataType: 'json',
        });
    }

    export function setUserProfile(profile: UserProfileContract): JQueryPromise<any> {
        return $.ajax({
            contentType: 'application/json',
            url: restRoot + 'userprofile',
            data: JSON.stringify(profile),
            type: 'PUT'
        });
    }

    export function updateSearchQueries(queries: string): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'userprofile?favorites',
            contentType: 'text/plain',
            data: queries,
            type: 'PUT'
        });
    }

    export function getPlanCurrent(): JQueryPromise<PlanCurrentContract[]> {
        return $.ajax({
            url: restRoot + 'plan',
            dataType: 'json',
        });
    }

    export function getPlanCurrentForMobile(): JQueryPromise<PlanCurrentContractMobile[]> {
        return $.ajax({
            url: restRoot + 'plan?mobile',
            dataType: 'json',
        });
    }

    export function getProfileJobInfos(device: string): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'profile/' + device + '?activeJobs',
            dataType: 'json',
        });
    }

    export function getGuideInfo(device: string): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'guide/' + device,
            dataType: 'json',
        });
    }

    export function getInfoJobs(): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'info?jobs',
            dataType: 'json',
        });
    }

    export function getProfileSources(device: string): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'profile/' + device,
            dataType: 'json',
        });
    }

    export function getSecuritySettings(): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'configuration?security',
            dataType: 'json',
        });
    }

    export function getDirectorySettings(): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'configuration?directory',
            dataType: 'json',
        });
    }

    export function getGuideSettings(): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'configuration?guide',
            dataType: 'json',
        });
    }

    export function getSourceScanSettings(): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'configuration?scan',
            dataType: 'json',
        });
    }

    export function getProfileSettings(): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'configuration?devices',
            dataType: 'json',
        });
    }

    export function getOtherSettings(): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'configuration?other',
            dataType: 'json',
        });
    }

    export function getSchedulerRules(): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'configuration?rules',
            dataType: 'json',
        });
    }

    export function getProtocolEntries(device: string, startDay: Date, endDay: Date): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'protocol/' + device + '?start=' + startDay.toISOString() + '&end=' + endDay.toISOString(),
            dataType: 'json',
        });
    }

    export function queryProgramGuide(filter: GuideFilterContract, protocolFilter: (key: string, value: any) => any): JQueryPromise<any> {
        return $.ajax({
            data: JSON.stringify(filter, protocolFilter),
            contentType: 'application/json',
            url: restRoot + 'guide',
            type: 'POST',
        });
    }

    export function countProgramGuide(filter: GuideFilterContract, protocolFilter: (key: string, value: any) => any): JQueryPromise<any> {
        return $.ajax({
            data: JSON.stringify(filter, protocolFilter),
            url: restRoot + 'guide?countOnly',
            contentType: 'application/json',
            type: 'POST',
        });
    }

    export function getRecordingDirectories(): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'info?directories',
            dataType: 'json',
        });
    }

    export function getGuideItem(device: string, source: string, start: Date, end: Date): JQueryPromise<VCRServer.GuideItemContract> {
        return $.ajax({
            url: restRoot + 'guide?profile=' + encodeURIComponent(device) + '&source=' + source + '&pattern=' + start.getTime() + '-' + end.getTime(),
            dataType: 'json',
        });
    }

    export function updateException(legacyId: string, referenceDay: string, startDelta: number, durationDelta: number): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'exception/' + legacyId + '?when=' + referenceDay + '&startDelta=' + startDelta + '&durationDelta=' + durationDelta,
            dataType: 'json',
            type: 'PUT'
        });
    }

    export function triggerTask(taskName: string): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'plan?' + taskName,
            dataType: 'json',
            type: 'POST',
        });
    }

    export function browseDirectories(root: string, children: boolean): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'configuration?browse&toParent=' + !children + '&root=' + encodeURIComponent(root),
            dataType: 'json',
        });
    }

    export function deleteSchedule(jobId: string, scheduleId: string): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'edit/' + jobId + scheduleId,
            contentType: 'application/json',
            type: 'DELETE',
        });
    }

    export function updateEndTime(device: string, suppressHibernate: boolean, scheduleIdentifier: string, newEnd: Date): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'profile/' + device + '?disableHibernate=' + suppressHibernate + '&schedule=' + scheduleIdentifier + '&endTime=' + newEnd.toISOString(),
            dataType: 'json',
            type: 'PUT',
        });
    }

    export function getWindowsGroups(): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'info?groups',
            dataType: 'json',
        });
    }

    export function updateConfiguration(type: string, contract: SettingsContract, protocolFilter: (key: string, value: any) => any = null): JQueryPromise<any> {
        if (protocolFilter == null)
            protocolFilter = function (key: string, value: any): any { return value; }

        return $.ajax({
            data: JSON.stringify(contract, protocolFilter),
            url: restRoot + 'configuration?' + type,
            contentType: 'application/json',
            type: 'PUT',
        });
    }

    export function validateDirectory(path: string): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'configuration?validate&directory=' + encodeURIComponent(path),
            dataType: 'json',
        });
    }

    export function createScheduleFromGuide(legacyId: string, epgId: string): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'edit/' + legacyId + epgId,
            dataType: 'json',
        });
    }

    export function getPlan(limit: number, end: Date): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'plan?limit=' + limit + '&end=' + end.toISOString(),
            dataType: 'json',
        });
    }

    export function getPlanForMobile(limit: number): JQueryPromise<any> {
        return $.ajax({
            url: restRoot + 'plan?limit=' + limit + '&mobile',
            dataType: 'json',
        });
    }

    export function updateSchedule(jobId: string, scheduleId: string, data: JobScheduleDataContract): JQueryPromise<any> {
        // Die Einstellungen, wie sie für das Neuanlegen benötigt werden
        var sendOptions: JQueryAjaxSettings =
            {
                contentType: 'application/json',
                data: JSON.stringify(data),
                url: restRoot + 'edit',
                type: 'POST',
            };

        // Eventuell existiert der Auftrag schon
        if (jobId != null) {
            sendOptions.url += '/' + jobId;

            // Eventuell existiert dann auch die Aufzeichnung
            if (scheduleId != null) {
                sendOptions.url += scheduleId;

                // Das wäre dann eine Aktualisierung
                sendOptions.type = 'PUT';
            }
        }

        // Befehl ausführen
        return $.ajax(sendOptions);
    }

    // Verwaltet die Aufzeichnungsverzeichnisse
    export class RecordingDirectoryCache {
        // Die zwischengespeicherten Verzeichnisse
        private static directories: string[] = null;

        // Vergisst alles, was wir wissen
        static reset(): void {
            RecordingDirectoryCache.directories = null;
        }

        // Ruft die Verzeichnisse ab
        static load(): JQueryPromise<any> {
            if (RecordingDirectoryCache.directories != null)
                return $.Deferred().resolve(RecordingDirectoryCache.directories);
            else
                return getRecordingDirectories().done(function (data: string[]): void { RecordingDirectoryCache.directories = data; });
        }
    }

    // Verwaltet die Geräteprofile
    export class ProfileCache {
        // Die zwischengespeicherten Geräte
        private static profiles: ProfileInfoContract[] = null;

        // Ruft die Profile ab
        static load(): JQueryPromise<any> {
            if (ProfileCache.profiles != null)
                return $.Deferred().resolve(ProfileCache.profiles);
            else
                return getProfileInfos().done(function (data: ProfileInfoContract[]): void { ProfileCache.profiles = data; });
        }
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

    // Verwaltet die Zusammenfassung der Daten der Programmzeitschrift für einzelne Geräte
    export class GuideInfoCache {
        private static guideInfoCache = {};

        // Meldet die Daten zu einem Gerät
        static getInfo(profileName: string): JQueryPromise<any> {
            var info: GuideInfoContract = GuideInfoCache.guideInfoCache[profileName];

            // Einmal laden reicht
            if (info != undefined)
                return $.Deferred().resolve(info)
            else
                return getGuideInfo(profileName).done(function (data: GuideInfoContract): void {
                    GuideInfoCache.guideInfoCache[profileName] = data;
                });
        }
    }

    // Beschreibt einen einzelne Quelle, so wie sie dem Anwender zur Auswahl angeboten wird
    export class SourceEntry {
        constructor(rawData: ProfileSourceContract) {
            var me = this;

            me.isTelevision = rawData.tvNotRadio;
            me.name = rawData.nameWithProvider;
            me.isEncrypted = rawData.encrypted;

            // Zum schnellen Auswählen nach dem Namen
            me.firstNameCharacter = me.name.toUpperCase().charAt(0);
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

    // Verwaltet Listen von Quellen zu Geräteprofilen
    export class SourceEntryCollection {
        // Verwaltet alle Quellen zu allen Geräten als Nachschlageliste
        private profileSources: any = {};

        // Fordert die Quellen eines Geräteprofils an.
        requestSources(profileName: string, whenDone: () => void): void {
            var me = this;

            // Eventuell haben wir das schon einmal gemacht
            if (me.getSourcesForProfile(profileName) != undefined)
                whenDone();
            else
                getProfileSources(profileName).done(function (data: ProfileSourceContract[]): void {
                    me.profileSources[profileName] = $.map(data, function (rawData: ProfileSourceContract): SourceEntry { return new SourceEntry(rawData); });

                    whenDone();
                });
        }

        // Meldet die aktuellen Quellen eines Gerätes.
        getSourcesForProfile(profileName: string): SourceEntry[] {
            if (profileName == null)
                return [];
            else
                return this.profileSources[profileName];
        }

        // Die einzige Instanz dieser Klasse
        static global = new SourceEntryCollection();
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
            var me = this;
            me.isLoaded = false;

            getUserProfile().done(function (data: UserProfileContract): void { me.loadFrom(data); });
        }

        // Übernimmt neue Daten.
        private loadFrom(data: UserProfileContract): void {
            var me = this;
            me.isLoaded = true;

            // Liste der Quelle in eine Nachschlagekarte umsetzen
            me.recentSources = {};
            $.each(data.recentSources, function (index: number, sourceName: string): void { me.recentSources[sourceName] = true; });

            // Das meiste wird unverändert übernommen
            me.hasRecentSources = data.recentSources.length > 0;
            me.maximumRecentSources = data.recentSourceLimit;
            me.noHibernateOnAbort = data.suppressHibernate;
            me.defaultEncryption = data.encryptionFilter;
            me.defaultAllLanguages = data.languages;
            me.defaultDVBSubtitles = data.subtitles;
            me.guidePreTime = data.guideAheadStart;
            me.guidePostTime = data.guideBeyondEnd;
            me.guideSearches = data.guideSearches;
            me.defaultVideotext = data.videotext;
            me.guideAfterAdd = data.backToGuide;
            me.planDaysToShow = data.planDays;
            me.defaultType = data.typeFilter;
            me.rowsInGuide = data.guideRows;
            me.defaultDolby = data.dolby;

            // Benachrichtigung auslösen
            if (me.loaded != null)
                me.loaded();
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
                .done(function (data: UserProfileContract): void { UserProfile.global.loadFrom(data); window.location.hash = 'home'; })
                .fail(JMSLib.dispatchErrorMessage(onError));
        }

        // Die einzige Instanz der Einstellungen.
        static global = new UserProfile();
    }

}

