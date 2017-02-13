/// <reference path='jmslib.ts' />
/// <reference path='../src/lib/http/config.ts' />

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

    export interface PlanCurrentContract {
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

    // Beschreibt einen Eintrag im Aufzeichnungsplan.
    export interface PlanActivityContract {
        // Beginn der Aufzeichnung im ISO Format.
        start?: string;

        // Dauer der Aufzeichung in Sekunden.
        duration: string;

        // Name der Aufzeichnung.
        name: string;

        // Gerät, auf dem aufgezeichnet wird.
        device?: string;

        // Sender, von dem aufgezeichnet wird.
        station?: string;

        // Gesetzt, wenn die Aufzeichnung verspätet beginnt.
        late: boolean;

        // Gesetzt, wenn die Aufzeichnung gar nicht ausgeführt wird.
        lost: boolean;

        // Gesetzt, wenn Informationen aus der Programmzeitschrift vorliegen.
        epg: boolean;

        // Das Gerät, in dessen Programmzeitschrift die Aufzeichnung gefunden wurde.
        epgDevice?: string;

        // Die Quelle zur Aufzeichnung in der Programzeitschrift.
        source?: string;

        // Die eindeutige Kennung der Aufzeichnung.
        id: string;

        // Gesetzt, wenn die Endzeit durch eine Sommer-/Winterzeitumstellung nicht korrekt ist.
        suspectEndTime: boolean;

        // Gesetzt, wenn alle Tonspuren aufgezeichnet werden sollen.
        allAudio: boolean;

        // Gesetzt, wenn Dolby Tonspuren aufgezeichnet werden sollen.
        ac3: boolean;

        // Gesetzt, wenn der Videotext mit aufgezeichnet werden soll.
        ttx: boolean;

        // Gesetzt, wenn DVB Untertitel mit aufgezeichnet werden sollen.
        dvbsub: boolean;

        // Gesetzt, wenn die Aufzeichnung laut Programmzeitschrift gerade läuft.
        epgCurrent: boolean;

        // Aktive Ausnahmeregel für die Aufzeichnung.
        exception?: PlanExceptionContract;
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

    // Gemeinsame Schnittstelle der Klassen EditSchedule und EditJob
    export interface EditJobScheduleCommonContract {
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
    }

    // Repräsentiert die Klasse EditSchedule
    export interface EditScheduleContract extends EditJobScheduleCommonContract {
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
    export interface EditJobContract extends EditJobScheduleCommonContract {
        // Das zu verwendende Aufzeichnungsverzeichnis
        directory: string;

        // Das zu verwendende Gerät
        device: string;

        // Gesetzt, wenn die Aufzeichnung auf jeden Fall auf dem angegebenen Geräte erfolgen soll
        lockedToDevice: boolean;
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

    // Die Art der zu suchenden Quelle
    export enum GuideSource {
        // Nur Fernsehsender
        TV = 1,

        // Nur Radiosender
        RADIO = 2,

        // Einfach alles
        ALL = TV + RADIO,
    }

    // Die Verschlüsselung der Quelle
    export enum GuideEncryption {
        // Nur kostenlose Quellen
        FREE = 1,

        // Nur Bezahlsender
        PAY = 2,

        // Alle Sender
        ALL = FREE + PAY,
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

        // Einschränkung auf die Art der Quellen
        typeFilter: GuideSource;

        // Einschränkung auf die Verschlüsselung der Quellen
        cryptFilter: GuideEncryption;
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
        return doUrlCall('userprofile');
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
        return doUrlCall("guide", "POST", filter);
    }

    export function countProgramGuide(filter: GuideFilterContract, protocolFilter: (key: string, value: any) => any): JQueryPromise<any> {
        return $.ajax({
            data: JSON.stringify(filter, protocolFilter),
            url: restRoot + 'guide?countOnly',
            contentType: 'application/json',
            type: 'POST',
        });
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
                .done((data: UserProfileContract) => { UserProfile.global.loadFrom(data); window.location.hash = 'home'; })
                .fail(JMSLib.dispatchErrorMessage(onError));
        }

        // Die einzige Instanz der Einstellungen.
        static global = new UserProfile();
    }

}

