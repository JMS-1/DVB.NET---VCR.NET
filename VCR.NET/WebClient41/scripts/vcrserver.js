/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='jmslib.ts' />
var VCRServer;
(function (VCRServer) {
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

    function getRestRoot() {
        return restRoot;
    }
    VCRServer.getRestRoot = getRestRoot;

    function getServerRoot() {
        return serverRoot;
    }
    VCRServer.getServerRoot = getServerRoot;

    function getFileRoot() {
        return playUrl;
    }
    VCRServer.getFileRoot = getFileRoot;

    function getDeviceRoot() {
        return deviceUrl;
    }
    VCRServer.getDeviceRoot = getDeviceRoot;

    function getServerVersion() {
        return $.ajax({
            url: restRoot + 'info',
            dataType: 'json'
        });
    }
    VCRServer.getServerVersion = getServerVersion;

    function getProfileInfos() {
        return $.ajax({
            url: restRoot + 'profile',
            dataType: 'json'
        });
    }
    VCRServer.getProfileInfos = getProfileInfos;

    function getUserProfile() {
        return $.ajax({
            url: restRoot + 'userprofile',
            dataType: 'json'
        });
    }
    VCRServer.getUserProfile = getUserProfile;

    function setUserProfile(profile) {
        return $.ajax({
            contentType: 'application/json',
            url: restRoot + 'userprofile',
            data: JSON.stringify(profile),
            type: 'PUT'
        });
    }
    VCRServer.setUserProfile = setUserProfile;

    function updateSearchQueries(queries) {
        return $.ajax({
            url: restRoot + 'userprofile?favorites',
            contentType: 'text/plain',
            data: queries,
            type: 'PUT'
        });
    }
    VCRServer.updateSearchQueries = updateSearchQueries;

    function getPlanCurrent() {
        return $.ajax({
            url: restRoot + 'plan',
            dataType: 'json'
        });
    }
    VCRServer.getPlanCurrent = getPlanCurrent;

    function getProfileJobInfos(device) {
        return $.ajax({
            url: restRoot + 'profile/' + device + '?activeJobs',
            dataType: 'json'
        });
    }
    VCRServer.getProfileJobInfos = getProfileJobInfos;

    function getGuideInfo(device) {
        return $.ajax({
            url: restRoot + 'guide/' + device,
            dataType: 'json'
        });
    }
    VCRServer.getGuideInfo = getGuideInfo;

    function getInfoJobs() {
        return $.ajax({
            url: restRoot + 'info?jobs',
            dataType: 'json'
        });
    }
    VCRServer.getInfoJobs = getInfoJobs;

    function getProfileSources(device) {
        return $.ajax({
            url: restRoot + 'profile/' + device,
            dataType: 'json'
        });
    }
    VCRServer.getProfileSources = getProfileSources;

    function getSecuritySettings() {
        return $.ajax({
            url: restRoot + 'configuration?security',
            dataType: 'json'
        });
    }
    VCRServer.getSecuritySettings = getSecuritySettings;

    function getDirectorySettings() {
        return $.ajax({
            url: restRoot + 'configuration?directory',
            dataType: 'json'
        });
    }
    VCRServer.getDirectorySettings = getDirectorySettings;

    function getGuideSettings() {
        return $.ajax({
            url: restRoot + 'configuration?guide',
            dataType: 'json'
        });
    }
    VCRServer.getGuideSettings = getGuideSettings;

    function getSourceScanSettings() {
        return $.ajax({
            url: restRoot + 'configuration?scan',
            dataType: 'json'
        });
    }
    VCRServer.getSourceScanSettings = getSourceScanSettings;

    function getProfileSettings() {
        return $.ajax({
            url: restRoot + 'configuration?devices',
            dataType: 'json'
        });
    }
    VCRServer.getProfileSettings = getProfileSettings;

    function getOtherSettings() {
        return $.ajax({
            url: restRoot + 'configuration?other',
            dataType: 'json'
        });
    }
    VCRServer.getOtherSettings = getOtherSettings;

    function getSchedulerRules() {
        return $.ajax({
            url: restRoot + 'configuration?rules',
            dataType: 'json'
        });
    }
    VCRServer.getSchedulerRules = getSchedulerRules;

    function getProtocolEntries(device, startDay, endDay) {
        return $.ajax({
            url: restRoot + 'protocol/' + device + '?start=' + startDay.toISOString() + '&end=' + endDay.toISOString(),
            dataType: 'json'
        });
    }
    VCRServer.getProtocolEntries = getProtocolEntries;

    function queryProgramGuide(filter, protocolFilter) {
        return $.ajax({
            data: JSON.stringify(filter, protocolFilter),
            contentType: 'application/json',
            url: restRoot + 'guide',
            type: 'POST'
        });
    }
    VCRServer.queryProgramGuide = queryProgramGuide;

    function countProgramGuide(filter, protocolFilter) {
        return $.ajax({
            data: JSON.stringify(filter, protocolFilter),
            url: restRoot + 'guide?countOnly',
            contentType: 'application/json',
            type: 'POST'
        });
    }
    VCRServer.countProgramGuide = countProgramGuide;

    function getRecordingDirectories() {
        return $.ajax({
            url: restRoot + 'info?directories',
            dataType: 'json'
        });
    }
    VCRServer.getRecordingDirectories = getRecordingDirectories;

    function getGuideItem(device, source, start, end) {
        return $.ajax({
            url: restRoot + 'guide?profile=' + device + '&source=' + source + '&pattern=' + start.getTime() + '-' + end.getTime(),
            dataType: 'json'
        });
    }
    VCRServer.getGuideItem = getGuideItem;

    function updateException(legacyId, referenceDay, startDelta, durationDelta) {
        return $.ajax({
            url: restRoot + 'exception/' + legacyId + '?when=' + referenceDay + '&startDelta=' + startDelta + '&durationDelta=' + durationDelta,
            dataType: 'json',
            type: 'PUT'
        });
    }
    VCRServer.updateException = updateException;

    function triggerTask(taskName) {
        return $.ajax({
            url: restRoot + 'plan?' + taskName,
            dataType: 'json',
            type: 'POST'
        });
    }
    VCRServer.triggerTask = triggerTask;

    function browseDirectories(root, children) {
        return $.ajax({
            url: restRoot + 'configuration?browse&toParent=' + !children + '&root=' + root,
            dataType: 'json'
        });
    }
    VCRServer.browseDirectories = browseDirectories;

    function deleteSchedule(jobId, scheduleId) {
        return $.ajax({
            url: restRoot + 'edit/' + jobId + scheduleId,
            contentType: 'application/json',
            type: 'DELETE'
        });
    }
    VCRServer.deleteSchedule = deleteSchedule;

    function updateEndTime(device, suppressHibernate, scheduleIdentifier, newEnd) {
        return $.ajax({
            url: restRoot + 'profile/' + device + '?disableHibernate=' + suppressHibernate + '&schedule=' + scheduleIdentifier + '&endTime=' + newEnd.toISOString(),
            dataType: 'json',
            type: 'PUT'
        });
    }
    VCRServer.updateEndTime = updateEndTime;

    function getWindowsGroups() {
        return $.ajax({
            url: restRoot + 'info?groups',
            dataType: 'json'
        });
    }
    VCRServer.getWindowsGroups = getWindowsGroups;

    function updateConfiguration(type, contract, protocolFilter) {
        if (typeof protocolFilter === "undefined") { protocolFilter = null; }
        if (protocolFilter == null)
            protocolFilter = function (key, value) {
                return value;
            };

        return $.ajax({
            data: JSON.stringify(contract, protocolFilter),
            url: restRoot + 'configuration?' + type,
            contentType: 'application/json',
            type: 'PUT'
        });
    }
    VCRServer.updateConfiguration = updateConfiguration;

    function validateDirectory(path) {
        return $.ajax({
            url: restRoot + 'configuration?validate&directory=' + path,
            dataType: 'json'
        });
    }
    VCRServer.validateDirectory = validateDirectory;

    function createScheduleFromGuide(legacyId, epgId) {
        return $.ajax({
            url: restRoot + 'edit/' + legacyId + epgId,
            dataType: 'json'
        });
    }
    VCRServer.createScheduleFromGuide = createScheduleFromGuide;

    function getPlan(limit, end) {
        return $.ajax({
            url: restRoot + 'plan?limit=' + limit + '&end=' + end.toISOString(),
            dataType: 'json'
        });
    }
    VCRServer.getPlan = getPlan;

    function updateSchedule(jobId, scheduleId, data) {
        // Die Einstellungen, wie sie für das Neuanlegen benötigt werden
        var sendOptions = {
            contentType: 'application/json',
            data: JSON.stringify(data),
            url: restRoot + 'edit',
            type: 'POST'
        };

        if (jobId != null) {
            sendOptions.url += '/' + jobId;

            if (scheduleId != null) {
                sendOptions.url += scheduleId;

                // Das wäre dann eine Aktualisierung
                sendOptions.type = 'PUT';
            }
        }

        // Befehl ausführen
        return $.ajax(sendOptions);
    }
    VCRServer.updateSchedule = updateSchedule;

    // Verwaltet die Aufzeichnungsverzeichnisse
    var RecordingDirectoryCache = (function () {
        function RecordingDirectoryCache() {
        }
        RecordingDirectoryCache.reset = // Vergisst alles, was wir wissen
        function () {
            RecordingDirectoryCache.directories = null;
        };

        RecordingDirectoryCache.load = // Ruft die Verzeichnisse ab
        function () {
            if (RecordingDirectoryCache.directories != null)
                return $.Deferred().resolve(RecordingDirectoryCache.directories);
else
                return getRecordingDirectories().done(function (data) {
                    RecordingDirectoryCache.directories = data;
                });
        };
        RecordingDirectoryCache.directories = null;
        return RecordingDirectoryCache;
    })();
    VCRServer.RecordingDirectoryCache = RecordingDirectoryCache;

    // Verwaltet die Geräteprofile
    var ProfileCache = (function () {
        function ProfileCache() {
        }
        ProfileCache.load = // Ruft die Profile ab
        function () {
            if (ProfileCache.profiles != null)
                return $.Deferred().resolve(ProfileCache.profiles);
else
                return getProfileInfos().done(function (data) {
                    ProfileCache.profiles = data;
                });
        };
        ProfileCache.profiles = null;
        return ProfileCache;
    })();
    VCRServer.ProfileCache = ProfileCache;

    // Beschreibt die Daten der Programmzeitschrift für ein Gerät
    var GuideInfo = (function () {
        function GuideInfo(rawData) {
            this.firstStart = (rawData.first == null) ? null : new Date(rawData.first);
            this.lastStart = (rawData.last == null) ? null : new Date(rawData.last);
            this.stations = rawData.stations;
        }
        return GuideInfo;
    })();
    VCRServer.GuideInfo = GuideInfo;

    // Verwaltet die Zusammenfassung der Daten der Programmzeitschrift für einzelne Geräte
    var GuideInfoCache = (function () {
        function GuideInfoCache() {
        }
        GuideInfoCache.getInfo = // Meldet die Daten zu einem Gerät
        function (profileName) {
            var info = GuideInfoCache.guideInfoCache[profileName];

            if (info != undefined)
                return $.Deferred().resolve(info);
else
                return getGuideInfo(profileName).done(function (data) {
                    GuideInfoCache.guideInfoCache[profileName] = data;
                });
        };
        GuideInfoCache.guideInfoCache = {};
        return GuideInfoCache;
    })();
    VCRServer.GuideInfoCache = GuideInfoCache;

    // Beschreibt einen einzelne Quelle, so wie sie dem Anwender zur Auswahl angeboten wird
    var SourceEntry = (function () {
        function SourceEntry(rawData) {
            var me = this;

            me.isTelevision = rawData.tvNotRadio;
            me.name = rawData.nameWithProvider;
            me.isEncrypted = rawData.encrypted;

            // Zum schnellen Auswählen nach dem Namen
            me.firstNameCharacter = me.name.toUpperCase().charAt(0);
        }
        return SourceEntry;
    })();
    VCRServer.SourceEntry = SourceEntry;

    // Verwaltet Listen von Quellen zu Geräteprofilen
    var SourceEntryCollection = (function () {
        function SourceEntryCollection() {
            // Verwaltet alle Quellen zu allen Geräten als Nachschlageliste
            this.profileSources = {};
        }
        // Fordert die Quellen eines Geräteprofils an.
        SourceEntryCollection.prototype.requestSources = function (profileName, whenDone) {
            var me = this;

            if (me.getSourcesForProfile(profileName) != undefined)
                whenDone();
else
                getProfileSources(profileName).done(function (data) {
                    me.profileSources[profileName] = $.map(data, function (rawData) {
                        return new SourceEntry(rawData);
                    });

                    whenDone();
                });
        };

        // Meldet die aktuellen Quellen eines Gerätes.
        SourceEntryCollection.prototype.getSourcesForProfile = function (profileName) {
            if (profileName == null)
                return [];
else
                return this.profileSources[profileName];
        };

        SourceEntryCollection.global = new SourceEntryCollection();
        return SourceEntryCollection;
    })();
    VCRServer.SourceEntryCollection = SourceEntryCollection;

    // Beschreibt die individuellen Einstellungen des Anwenders
    var UserProfile = (function () {
        function UserProfile() {
            // Die Anzahl der Vorschautage im Aufzeichnungsplan
            this.planDaysToShow = 0;
            // Gesetzt, wenn die Liste der bisher verwendeten Quellen nicht leer ist
            this.hasRecentSources = false;
            // Alle bisher verwendeten Quellen als Nachschlageliste
            this.recentSources = {};
            // Die bevorzugte Auswahl der Art der Quellen (R=Radio, T=Fernsehen)
            this.defaultType = 'RT';
            // Die bevorzugte Auswahl der Verschlüsselung der Quellen (F=unverschlüsselt, P=verschlüsselt)
            this.defaultEncryption = 'FP';
            // Gesetzt, wenn bevorzugt alle Sprachen aufgezeichnet werden sollen
            this.defaultAllLanguages = false;
            // Gesetzt, wenn bevorzugt die Dolby Digital Tonspur mit aufgezeichnet werden soll
            this.defaultDolby = false;
            // Gesetzt, wenn bevorzugt der Videotext mit aufgezeichnet werden soll
            this.defaultVideotext = false;
            // Gesetzt, wenn bevorzugt DVB Untertitel mit aufgezeichnet werden sollen
            this.defaultDVBSubtitles = false;
            // Gesetzt, wenn nach dem Anlegen einer Aufzeichnung aus der Programmzeitschrift diese wieder aktiviert werden soll
            this.guideAfterAdd = false;
            // Die Anzahl der Sendungen pro Seite der Programmzeitschrift
            this.rowsInGuide = 30;
            // Die Vorlaufzeit (in Minuten) von Aufzeichnungen bei der Programmierung über die Programmzeitschrift
            this.guidePreTime = 15;
            // Die Nachlaufzeit (in Minuten) von Aufzeichnungen bei der Programmierung über die Programmzeitschrift
            this.guidePostTime = 30;
            // Die maximale Anzahl von Quellen in der Liste der zuletzt verwendeten Quellen
            this.maximumRecentSources = 10;
            // Gesetzt, wenn bevorzugt beim Abbruch einer laufenden Aufzeichnung oder Aufgabe der Übergang in den Schlafzustand deaktiviert werden soll
            this.noHibernateOnAbort = false;
            // Die gespeicherten Suchen der Programmzeitschrift
            this.guideSearches = null;
            // Wird gesetzt, sobald Daten zur Verfügung stehen
            this.isLoaded = false;
            // Wird aufgerufen, sobald Daten zur Verfügung stehen
            this.loaded = null;
        }
        // Meldet Interesse am Ladezustand an
        UserProfile.prototype.register = function (whenLoaded) {
            this.loaded = whenLoaded;

            if (this.isLoaded)
                if (whenLoaded != null)
                    whenLoaded();
        };

        // Ruft eine aktuelle Konfiguration vom Web Service ab.
        UserProfile.prototype.refresh = function () {
            var me = this;
            me.isLoaded = false;

            getUserProfile().done(function (data) {
                me.loadFrom(data);
            });
        };

        // Übernimmt neue Daten.
        UserProfile.prototype.loadFrom = function (data) {
            var me = this;
            me.isLoaded = true;

            // Liste der Quelle in eine Nachschlagekarte umsetzen
            me.recentSources = {};
            $.each(data.recentSources, function (index, sourceName) {
                me.recentSources[sourceName] = true;
            });

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

            if (me.loaded != null)
                me.loaded();
        };

        // Erstellt die Repräsentation, die auch an den Server geschickt werden kann.
        UserProfile.prototype.toContract = function () {
            var profileData = {
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
                dolby: this.defaultDolby
            };

            return profileData;
        };

        // Erstellt eine fast exakte Kopie - die Liste der zuletzt verwendeten Sourcen wird nicht benötigt.
        UserProfile.prototype.clone = function () {
            var profile = new UserProfile();

            profile.loadFrom(this.toContract());

            return profile;
        };

        // Sendet die aktuelle Konfiguration an den Web Service.
        UserProfile.prototype.update = function (onError) {
            setUserProfile(this.toContract()).done(function (data) {
                UserProfile.global.loadFrom(data);
                window.location.hash = 'home';
            }).fail(JMSLib.dispatchErrorMessage(onError));
        };

        UserProfile.global = new UserProfile();
        return UserProfile;
    })();
    VCRServer.UserProfile = UserProfile;
})(VCRServer || (VCRServer = {}));
//# sourceMappingURL=vcrserver.js.map
