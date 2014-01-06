/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='typings/jqueryui/jqueryui.d.ts' />
/// <reference path='vcrserver.ts' />
/// <reference path='jmslib.ts' />
var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
// Alle CSS Klassen, die vom Code aus gesetzt werden
var CSSClass = (function () {
    function CSSClass() {
    }
    CSSClass.inactiveOptionClass = 'inactiveRecordingOption';

    CSSClass.blink = 'highlightText';

    CSSClass.danger = 'danger';

    CSSClass.faq = 'faqContent';

    CSSClass.editLabel = 'editLabel';

    CSSClass.editSpacing = 'spaceColumn';

    CSSClass.noJobText = 'jobTextPlaceholder';

    CSSClass.jobText = 'jobText';

    CSSClass.badEndTime = 'suspectEnd';

    CSSClass.fullRecord = 'guideInsidePlan';

    CSSClass.partialRecord = 'guideOutsidePlan';
    return CSSClass;
})();

// Beschreibt einen Favoritensuche in der Programmzeitschrift
var SavedGuideQuery = (function () {
    function SavedGuideQuery(rawQuery) {
        if (typeof rawQuery === "undefined") { rawQuery = null; }
        // Optional die Quelle
        this.source = null;
        // Die Anzahl der Sendungen zur Suche
        this.cachedCount = null;
        // Gesetzt, während die Anzahl geladen wird
        this.loadingCount = false;
        var me = this;

        // Das deserialisierte Objekt sieht aus wie wir hat aber nur Eigenschaften
        if (rawQuery != null) {
            me.titleOnly = rawQuery.titleOnly;
            me.device = rawQuery.device;
            me.source = rawQuery.source;
            me.text = rawQuery.text;
        }

        // Der Einfachheit halber verwenden wir eine Umlenkung von Benutzeraktionen auf statische Methode
        me.onDelete = function () {
            SavedGuideQuery.onDeleted(me);
        };
        me.onExecute = function () {
            SavedGuideQuery.onClick(me);
        };
    }
    // Aktiviert den nächsten Ladevorgang
    SavedGuideQuery.dispatchLoad = function () {
        if (SavedGuideQuery.loading)
            return;
        if (SavedGuideQuery.loadQueue.length < 1)
            return;

        // Auftrag entfernen
        var query = SavedGuideQuery.loadQueue.splice(0, 1)[0];

        // Filter zusammenbauen
        var filter = new GuideFilter();
        filter.station = query.source;
        filter.device = query.device;
        filter.title = query.text;
        filter.size = 0;
        if (!query.titleOnly)
            filter.content = query.text;

        SavedGuideQuery.loading = true;

        // Ergebnis ermitteln
        filter.count(function (count) {
            // Nächsten Eintrag der Warteschlange anwerfen
            SavedGuideQuery.loading = false;

            SavedGuideQuery.dispatchLoad();

            // Ergebnis übernehmen
            query.cachedCount = count;
            query.loadingCount = false;

            // Und mal schauen, ob jemand darauf wartet
            if (SavedGuideQuery.onCountLoaded != null)
                SavedGuideQuery.onCountLoaded(query);
        });
    };

    // Die aktuelle Anzahl von Sendungen zur Suche
    SavedGuideQuery.prototype.count = function () {
        // Das haben wir bereits gemacht
        if (this.cachedCount != null)
            return this.cachedCount;

        // Wir haben noch nie gefragt
        if (!this.loadingCount) {
            this.loadingCount = true;

            SavedGuideQuery.loadQueue.push(this);
            SavedGuideQuery.dispatchLoad();
        }

        return '-';
    };

    // Stellt sicher, dass einige Eigenschaften nicht serialisiert werden
    SavedGuideQuery.filterProperties = function (key, value) {
        if (key == 'onExecute')
            return undefined;
        if (key == 'onDelete')
            return undefined;
        if (key == 'cachedCount')
            return undefined;
        if (key == 'loadingCount')
            return undefined;

        return value;
    };

    // Erstellt eine Textdarstellung
    SavedGuideQuery.prototype.displayText = function () {
        var display = 'Alle Sendungen, die über das Gerät ' + this.device;

        // Die Quelle ist optional
        if (this.source != null)
            if (this.source.length > 0) {
                display += ' von der Quelle ';
                display += this.source;
            }

        display += ' empfangen werden und deren Name ';
        if (!this.titleOnly)
            display += 'oder Beschreibung ';

        // Auch wenn wir das jetzt nicht unterstützen gibt es auch die Suche nach exakter Gleichheit
        if (this.text[0] != '*')
            display += 'mit';

        display += ' "';
        display += this.text.substr(1);
        display += '" ';

        if (this.text[0] == '*')
            display += 'enthält';
        else
            display += 'ist';

        return display;
    };
    SavedGuideQuery.loadQueue = new Array();

    SavedGuideQuery.loading = false;

    SavedGuideQuery.onClick = null;

    SavedGuideQuery.onDeleted = null;

    SavedGuideQuery.onCountLoaded = null;
    return SavedGuideQuery;
})();

// Die Verwaltung aller gespeicherten Suchen
var SavedGuideQueries = (function () {
    function SavedGuideQueries() {
    }
    // Ermittelt alle gespeicherten Suchen
    SavedGuideQueries.load = function () {
        // Bereits geladen
        var cache = SavedGuideQueries.queries;
        if (cache != undefined)
            return cache;

        // Aus dem Benutzerprofil übernehmen (das ist immer schon geladen) - eventuell aus VCR.NET 4.1 migrieren
        var fromStore = VCRServer.UserProfile.global.guideSearches;
        if (fromStore.length < 1) {
            fromStore = localStorage.getItem(SavedGuideQueries.storeName);
            if (fromStore == null)
                cache = new Array();
            else
                cache = JSON.parse(fromStore);
        } else
            cache = JSON.parse(fromStore);

        // In echte Objekte mit Methoden wandeln
        SavedGuideQueries.queries = $.map(cache, function (rawQuery) {
            return new SavedGuideQuery(rawQuery);
        });

        return SavedGuideQueries.queries;
    };

    // Aktualisiert die gespeicherten Suchen
    SavedGuideQueries.save = function () {
        VCRServer.updateSearchQueries(JSON.stringify(SavedGuideQueries.queries, SavedGuideQuery.filterProperties));
    };
    SavedGuideQueries.storeName = 'vcrnet.guidequeries';
    return SavedGuideQueries;
})();

// Repräsentiert die Klasse GuideFilter
var GuideFilter = (function () {
    function GuideFilter() {
        // Der Name des aktuell ausgewählten Geräteprofils
        this.device = null;
        // Der Name der aktuell ausgewählten Quelle
        this.station = null;
        // Der minimale Startzeitpunkt in ISO Notation
        this.start = null;
        // Das Suchmuster für den Namen einer Sendung
        this.title = null;
        // Das Suchmuster für die Beschreibung einer Sendung
        this.content = null;
        // Die Anzahl von Sendungen pro Anzeigeseite
        this.size = 10;
        // Die aktuelle Seite
        this.index = 0;
        // Wird aufgerufen, wenn sich etwas verändert hat
        this.onChange = null;
        // Die aktuelle Anmeldung zum verzögerten Aufruf
        this.timeout = null;
        // Gesetzt, wenn der letzte Abruf zusätzliche Daten ermittelt hat
        this.moreAvailable = false;
    }
    // Meldet, ob ein Blättern nach vorne unterstützt wird
    GuideFilter.prototype.canPageForward = function () {
        return this.moreAvailable;
    };

    // Deaktiviert verzögerte Anforderungen
    GuideFilter.prototype.cancelTimeout = function () {
        if (this.timeout == null)
            return;

        window.clearTimeout(this.timeout);

        this.timeout = null;
    };

    // Meldet Änderungen an der Konfiguration
    GuideFilter.prototype.fireChange = function () {
        this.cancelTimeout();

        if (this.onChange != null)
            this.onChange();
    };

    // Reagiert auf die Änderung der Benutzereinstellungen
    GuideFilter.prototype.userProfileChanged = function () {
        var newSize = VCRServer.UserProfile.global.rowsInGuide;
        if (newSize == this.size)
            return;

        this.size = newSize;
        this.index = 0;
    };

    // Wechselt zur ersten Seite der Anzeige
    GuideFilter.prototype.firstPage = function () {
        this.index = 0;
        this.fireChange();
    };

    // Wechselt zur nächsten Seite
    GuideFilter.prototype.nextPage = function () {
        if (this.moreAvailable)
            this.index += 1;
        this.fireChange();
    };

    // Wechselt zur vorherigen Seite
    GuideFilter.prototype.prevPage = function () {
        if (this.index > 0)
            this.index -= 1;
        this.fireChange();
    };

    // Ändert das aktuelle Gerät
    GuideFilter.prototype.changeDevice = function (newDevice, newStation) {
        if (typeof newStation === "undefined") { newStation = null; }
        if (newDevice != this.device) {
            this.device = newDevice;
            this.index = 0;
        }

        if (newStation != null)
            if (newStation != this.station) {
                this.station = newStation;
                this.index = 0;
            }

        this.fireChange();
    };

    // Ändert die aktuell ausgewählten Quelle
    GuideFilter.prototype.changeStation = function (newStation) {
        this.station = newStation;
        this.index = 0;
        this.fireChange();
    };

    GuideFilter.prototype.changeQuery = function (newText, withContent) {
        // Suchtext in Suchmuster umsetzen
        if (newText.length < 1)
            newText = null;
        else
            newText = '*' + newText;

        // Neue Suchmuster ermitteln und gegen die bisherigen prüfen
        var newContent = withContent ? newText : null;
        var newTitle = newText;
        if (this.title == newTitle)
            if (this.content == newContent)
                return;

        // Neue Suchmuster anwenden
        this.content = newContent;
        this.title = newTitle;
        this.index = 0;

        // Ausstehende verzögerte Aktualisierung deaktiviern
        if (this.timeout != null)
            window.clearTimeout(this.timeout);

        // Verzögerte Aktualisierung aktiveren
        var me = this;
        me.timeout = window.setTimeout(function () {
            me.fireChange();
        }, 200);
    };

    // Ändert den Startzeitpunkt
    GuideFilter.prototype.changeStart = function (newStart) {
        this.start = newStart;
        this.index = 0;
        this.fireChange();
    };

    // Entfernt alle Suchbedingungen
    GuideFilter.prototype.reset = function () {
        this.station = null;
        this.content = null;
        this.title = null;
        this.start = null;
        this.index = 0;
        this.fireChange();
    };

    // Sorgt dafür, dass unsere internen Eigenschaften nicht zum Server gesendet werden
    GuideFilter.filterProperties = function (key, value) {
        return ((key == 'timeout') || (key == 'moreAvailable') || (key == 'onChange')) ? undefined : value;
    };

    // Fordert neue Daten an
    GuideFilter.prototype.execute = function (whenLoaded) {
        var me = this;

        me.cancelTimeout();

        VCRServer.queryProgramGuide(me, GuideFilter.filterProperties).done(function (data) {
            var items = $.map(data, function (rawData) {
                return new GuideItem(rawData);
            });

            // Wir erhalten immer einen Eintrag mehr als angefordert, falls noch mehr Einträge existieren
            me.moreAvailable = items.length > me.size;

            whenLoaded(items.slice(0, me.size));
        });
    };

    // Fordert die Anzahl der Sendungen an
    GuideFilter.prototype.count = function (whenLoaded) {
        VCRServer.countProgramGuide(this, GuideFilter.filterProperties).done(whenLoaded);
    };

    GuideFilter.global = new GuideFilter();
    return GuideFilter;
})();

// Repräsentiert die Klasse JobScheduleData
var JobScheduleDataContract = (function () {
    function JobScheduleDataContract(job, schedule) {
        this.job = job;
        this.schedule = schedule;
    }
    // Aktualisiert eine Aufzeichnung
    JobScheduleDataContract.prototype.update = function (jobId, scheduleId, finishHash, onError) {
        VCRServer.updateSchedule(jobId, scheduleId, this).done(function () {
            VCRServer.UserProfile.global.refresh();
            window.location.hash = finishHash;
        }).fail(JMSLib.dispatchErrorMessage(onError));
    };
    return JobScheduleDataContract;
})();


// Basisklasse für alle Teilanwendungen
var Page = (function () {
    function Page() {
        // Alle noch ausstehenden asynchronen Initialisierungsaufrufe
        this.pending = 1;
        // Verwaltet asynchrone Aktualisierungsaufrufe
        this.nextPending = 2;
    }
    // Meldet eine asynchrone Initialisierung an
    Page.prototype.registerAsyncCall = function () {
        var me = this;
        var mask = me.nextPending;

        me.nextPending += mask;
        me.pending |= mask;

        return function () {
            me.finishedAsyncCall(mask);
        };
    };

    // Wird aufgerufen, wenn eine asynchrone Initialisierung abgeschlossen wurde
    Page.prototype.finishedAsyncCall = function (mask) {
        // Das machen wir nur ein einziges Mal
        if (this.pending == 0)
            return;

        // Fertigen Aufruf anmelden
        this.pending &= ~mask;

        // Wenn alles bereit steht anzeigen lassen
        if (this.pending == 0)
            this.show();
    };

    // Wird einmalig zur Vorbereitung der Anzeige aufgerufen
    Page.prototype.initialize = function (mainContent) {
        this.mainContent = mainContent;

        this.onInitialize();

        this.finishedAsyncCall(1);
    };

    // Wird unmittelbar vor der Anzeige aufgerufen
    Page.prototype.onShow = function () {
    };

    // Wird einmalig zur Anzeige aufgerufen
    Page.prototype.show = function () {
        var page = this;

        // Aktionen zurücksetzen
        $('.refreshLink').off('click');

        page.onShow();

        // Vorbereitung des Bereichs, der allen Seiten gemeinsam ist (Überschrift und Verweisleiste)
        $('#headline').text(page.title);

        var links = page.visibleLinks;
        if (links != '')
            $('.linkArea, ' + links).removeClass(JMSLib.CSSClass.invisible);

        // Eingebettete Hilfe vorbereiten
        JMSLib.activateHelp();

        // Nun endlich alles anzeigen
        this.mainContent.removeClass(JMSLib.CSSClass.invisible);
    };

    // Lädt die Daten zu allen Geräten
    Page.prototype.loadProfiles = function (target) {
        var me = this;
        var whenDone = me.registerAsyncCall();

        VCRServer.ProfileCache.load().done(function (profiles) {
            $.each(profiles, function (index, profile) {
                target.append(new Option(profile.name));

                VCRServer.SourceEntryCollection.global.requestSources(profile.name, me.registerAsyncCall());
            });

            whenDone();
        });
    };
    return Page;
})();

// Globale Initialisierungen
$(function () {
    // Benutzereinstellungen einmalig anfordern
    VCRServer.UserProfile.global.refresh();

    // Informationsdaten ermitteln
    VCRServer.getServerVersion().done(function (data) {
        $('#masterTitle').text('VCR.NET Recording Service ' + data.version);
    });

    // Hier kommt der Inhalt hin
    var mainContent = $('#mainArea');
    var tagClass = '.initialDisable';

    // Alles, was unsichtbar ist, zusätzlich markieren
    $('.' + JMSLib.CSSClass.invisible).addClass(tagClass.substr(1));

    // Eine Teilanwendung laden
    function loadMain(template) {
        // Zustand zurück setzen - es ist teilweise einfach, an globale statische Methoden zu binden, wenn die zugehörigen Daten auch global gehalten werden
        VCRServer.UserProfile.global.register(null);
        SavedGuideQuery.onCountLoaded = null;
        GuideFilter.global.onChange = null;
        SavedGuideQuery.onDeleted = null;
        SavedGuideQuery.onClick = null;

        // Anwender informieren und Seite anfordern
        $('#headline').text('(bitte einen Moment Geduld)');

        JMSLib.TemplateLoader.loadAbsolute('ui/' + template + '.html').done(function (wholeFile) {
            var content = $(wholeFile).find('#mainContent');
            var starterClass = template + 'Page';
            var starter = new window[starterClass];

            // Alles, was initial unsichtbar war, nun auf unsichtbar schalten
            $(tagClass).addClass(JMSLib.CSSClass.invisible);

            // Inhalt der Seite laden
            mainContent.html(null);
            mainContent.html(content.html());

            // Individuelle Initalisierung aufrufen
            starter.initialize(mainContent);
        });
    }

    // Gerade angezeigte Teilanwendung
    var currentHash = '';

    // Aktuelle Teilanwendung ermitteln und laden
    window.onhashchange = function (ev) {
        var hash = window.location.hash;
        if (hash.indexOf('#') != 0)
            hash = '#home';

        var app = hash.substr(1);
        var endOfApp = app.indexOf(';');
        if (endOfApp >= 0)
            app = app.substr(0, endOfApp);

        if (currentHash == app)
            if (currentHash != 'faq')
                if (currentHash != 'guide')
                    return;

        loadMain(currentHash = app);
    };

    // Aktuelle Seite laden
    window.onhashchange(null);
});

// Verwaltet die Auswahl einer Quelle für die Aufzeichnung, einschließlich der Aufzeichnungsoptionen
var SourceSelector = (function () {
    function SourceSelector(loader, sibling, pure) {
        var me = this;

        // Erst einmal eine Kopie aus den HTML Vorlagen erstellen
        me.loader = loader;
        me.options = loader.optionTemplate.clone();
        me.source = loader.sourceTemplate.clone();

        // Diese Kopien an der gewünschten Stelle in der Oberfläche einblenden
        if (pure)
            me.source.find('.' + CSSClass.editLabel + ', .' + CSSClass.editSpacing).remove();
        else
            me.options.insertAfter(sibling);
        me.source.insertAfter(sibling);

        // Alles, was wir öfter brauchen werden, einmal nachschlagen
        me.encryptionMode = me.source.find('#filterEncryption');
        me.sourceSelectionList = me.source.find('#selSource');
        me.sourceNameField = me.source.find('#sourceName');
        me.nameMode = me.source.find('#filterName');
        me.typeMode = me.source.find('#filterType');

        // Kleine Hilfe
        var refresh = function () {
            me.load();
        };

        // Alle Änderungen überwachen
        me.sourceSelectionList.change(function () {
            me.sourceNameField.val(me.sourceSelectionList.val());
            me.sourceNameField.change();
        });
        me.sourceNameField.change(function () {
            var selectedSource = me.sourceNameField.val();

            me.sourceSelectionList.val(selectedSource);
            me.options.find('input').prop('disabled', selectedSource.length < 1);
        });
        me.encryptionMode.change(refresh);
        me.typeMode.change(refresh);
        me.nameMode.change(refresh);
    }
    // Aktualisiert die Auswahlliste der Quellen.
    SourceSelector.prototype.load = function (firstCall) {
        if (typeof firstCall === "undefined") { firstCall = false; }
        var me = this;

        // Auswahllisten einblenden
        me.typeMode.removeClass(JMSLib.CSSClass.invisible);
        me.encryptionMode.removeClass(JMSLib.CSSClass.invisible);

        // Liste der Quellen ermitteln
        var profile = me.loader.profileSelector.val();
        var sources = VCRServer.SourceEntryCollection.global.getSourcesForProfile(profile);

        // Aktuelle Quelle sichern - diese kommt unabhängig vom Filter immer in die Liste
        var currentSource = me.sourceNameField.val();

        // Filter ermitteln
        var nameFilterMode = me.nameMode.val();
        var nameFilter;
        var filterOutTelevision;
        var filterOutEncrypted;

        switch (me.encryptionMode.val()) {
            case 'P':
                filterOutEncrypted = false;
                break;
            case 'F':
                filterOutEncrypted = true;
                break;
        }
        switch (me.typeMode.val()) {
            case 'T':
                filterOutTelevision = false;
                break;
            case 'R':
                filterOutTelevision = true;
                break;
        }

        if (nameFilterMode == '*')
            nameFilter = function (source) {
                return true;
            };
        else if (nameFilterMode == '!')
            nameFilter = function (source) {
                var first = source.firstNameCharacter;

                if (first < '0')
                    return true;
                else if (first > 'Z')
                    return true;
                else
                    return (first > '9') && (first < 'A');
            };
        else if (nameFilterMode.length == 2)
            nameFilter = function (source) {
                var first = source.firstNameCharacter;

                return (first >= nameFilterMode[0]) && (first <= nameFilterMode[1]);
            };
        else {
            // Immer alle Quellen anzeigen
            filterOutTelevision = undefined;
            filterOutEncrypted = undefined;

            // Zugehörige Auswahllisten ausblenden
            me.typeMode.addClass(JMSLib.CSSClass.invisible);
            me.encryptionMode.addClass(JMSLib.CSSClass.invisible);

            nameFilter = function (source) {
                return VCRServer.UserProfile.global.recentSources[source.name];
            };
        }

        // Zurück auf den Anfang der Liste
        me.sourceSelectionList.children().remove();
        me.sourceSelectionList.append(new Option('(Keine Quelle)', ''));

        // Liste der Quellen gemäß Filter füllen
        $.each(sources, function (index, source) {
            if (source.name != currentSource) {
                if (source.isEncrypted == filterOutEncrypted)
                    return;
                if (source.isTelevision == filterOutTelevision)
                    return;
                if (!nameFilter(source))
                    return;
            }

            me.sourceSelectionList.append(new Option(source.name));
        });

        // Auswahl aktualisieren
        me.sourceNameField.change();
        me.sourceSelectionList.change();

        // Beim ersten Aufruf sind wir fertig
        if (firstCall)
            return;

        // Das Label zur Auswahl der Quellen
        var label = me.source.children().first();

        // Etwas Feedback über die (eventuell nicht optisch sichtbare) Änderung der Auswahl geben
        label.addClass(CSSClass.blink);

        // Nach kurzer Zeit zurück auf den Normalzustand
        setTimeout(function () {
            label.removeClass(CSSClass.blink);
        }, 250);
    };

    // Setzt die Auswahl der Quelle zurück.
    SourceSelector.prototype.reset = function () {
        this.sourceSelectionList.val(null);
    };

    // Bereitet die Nutzung der Auswahl einmalig vor, nachdem alle benötigten Elemente für die Anzeige der Oberfläche geladen wurden.
    SourceSelector.prototype.initialize = function () {
        var me = this;

        // Einstellung des Anwenders auslesen und in die Oberfläche übertragen
        me.encryptionMode.val(VCRServer.UserProfile.global.defaultEncryption);
        me.typeMode.val(VCRServer.UserProfile.global.defaultType);
        if (!VCRServer.UserProfile.global.hasRecentSources) {
            // Es gibt gar keine Liste von zuletzt verwendeten Quellen
            me.nameMode.children().first().remove();
            me.nameMode.children().last().attr('selected', 'selected');
        }

        // Auswahl am Geräteprofil überwachen
        me.loader.profileSelector.change(function () {
            me.load();
        });

        // Erstmalig die Liste der Quellen laden
        me.load(true);
    };
    return SourceSelector;
})();

// Verwaltet die Erzeugung der Quellenauswahl
var SourceSelectorLoader = (function () {
    function SourceSelectorLoader(profile) {
        this.profileSelector = profile;
    }
    // Fordert die HTML Vorlagen vom Server an.
    SourceSelectorLoader.prototype.loadTemplates = function (success) {
        var me = this;

        // Laden anstossen
        JMSLib.TemplateLoader.load('sourceSelection').done(function (template) {
            var templateDom = $(template);

            me.optionTemplate = templateDom.find('#options');
            me.sourceTemplate = templateDom.find('#source');

            success();
        });
    };

    // Bereitet die Oberfläche für die Auswahl der Quelle vor.
    SourceSelectorLoader.prototype.appendAfter = function (sibling, pure) {
        if (typeof pure === "undefined") { pure = false; }
        return new SourceSelector(this, sibling, pure);
    };
    return SourceSelectorLoader;
})();

// Algorithmen zur Prüfung der Eingaben der Benutzereinstellungen
var UserSettingsValidator = (function () {
    function UserSettingsValidator(send) {
        // Wir arbeiten immer auf einer Kopie
        var clone = VCRServer.UserProfile.global.clone();

        this.sendButton = send;
        this.model = clone;

        // Overfläche vorbereiten
        send.button();
        send.click(function () {
            send.button('option', 'disabled', true);

            clone.update(function (error) {
                JMSLib.Bindings.setErrorIndicator(send, error);
            });
        });
    }
    // Prüft die Konsistenz von Eingabedaten
    UserSettingsValidator.prototype.validate = function () {
        var me = this;
        var profile = me.model;

        // Prüfen
        me.maximumRecentSources = JMSLib.Bindings.checkNumber(profile.maximumRecentSources, 1, 50);
        me.planDaysToShow = JMSLib.Bindings.checkNumber(profile.planDaysToShow, 1, 50);
        me.guidePostTime = JMSLib.Bindings.checkNumber(profile.guidePostTime, 0, 240);
        me.guidePreTime = JMSLib.Bindings.checkNumber(profile.guidePreTime, 0, 240);
        me.rowsInGuide = JMSLib.Bindings.checkNumber(profile.rowsInGuide, 10, 100);

        // Ergebnis der Prüfung
        var isValid = (me.planDaysToShow == null) && (me.maximumRecentSources == null) && (me.guidePreTime == null) && (me.guidePostTime == null) && (me.rowsInGuide == null);

        // Schaltfläche anpassen
        me.sendButton.button('option', 'disabled', !isValid);
    };
    return UserSettingsValidator;
})();

// Beschreibt einen einzelnen Eintrag der Programmzeitschrift, der zur Anzeige vorbereitet wurde
var GuideItem = (function () {
    function GuideItem(rawData) {
        // Wird aufgerufen, wenn der Anwender die Detailanzeige aktiviert hat
        this.onShowDetails = function (item, origin) {
        };
        // Eine CSS Klasse, die den Überlapp einer Aufzeichnung mit dem Eintrag ausdrückt.
        this.overlapClass = CSSClass.partialRecord;
        // Referenz besorgen
        var me = this;

        // Zeiten umrechnen
        var duration = rawData.duration * 1000;
        var start = new Date(rawData.start);
        var end = new Date(start.getTime() + duration);

        // Daten aus der Rohdarstellung in das Modell kopieren
        me.displayDuration = JMSLib.DateFormatter.getDuration(new Date(duration));
        me.inactiveClass = rawData.active ? '' : JMSLib.CSSClass.invisible;
        me.displayStart = JMSLib.DateFormatter.getStartTime(start);
        me.displayEnd = JMSLib.DateFormatter.getEndTime(end);
        me.short = rawData.shortDescription;
        me.language = rawData.language;
        me.long = rawData.description;
        me.station = rawData.station;
        me.name = rawData.name;
        me.duration = duration;
        me.id = rawData.id;
        me.start = start;
        me.end = end;

        // Listen
        var categories = rawData.categories;
        var ratings = rawData.ratings;

        me.categories = categories.join(' ');
        me.ratings = ratings.join(' ');

        // Detailanzeige immer aktivieren
        me.showDetails = function () {
            me.onShowDetails(me, this);
        };

        // Suche immer aktivieren
        me.findInGuide = function () {
            var filter = GuideFilter.global;

            // Ganz von vorne
            filter.reset();

            // Textsuche auf den Namen auf der selben Karte
            filter.device = me.id.split(':')[1];
            filter.title = '=' + me.name;
            filter.station = me.station;

            // Aufrufen
            if (window.location.hash == '#guide')
                window.location.hash = '#guide;auto';
            else
                window.location.hash = '#guide';
        };
    }
    return GuideItem;
})();

// Verwaltet einen Eintrag aus der Programmzeitschrift und stellt sicher, dass dieser nur einmal angefordert wird
var GuideItemCache = (function () {
    function GuideItemCache() {
    }
    GuideItemCache.prototype.request = function (device, source, start, end, dataAvailable) {
        var me = this;

        // Haben wir schon
        if (me.guideItem == undefined)
            VCRServer.getGuideItem(device, source, start, end).done(function (rawData) {
                if (rawData == null)
                    me.guideItem = null;
                else
                    dataAvailable(me.guideItem = new GuideItem(rawData));
            });
        else if (me.guideItem != null)
            dataAvailable(me.guideItem);
    };
    return GuideItemCache;
})();

// Beschreibt einen einzelnen Eintrag in Aufzeichnungsplan
var PlanEntry = (function () {
    function PlanEntry(rawData) {
        // Wird aufgerufen, wenn der Anwender die Detailanzeige aktiviert hat.
        this.onShowDetails = function (item, origin) {
        };
        // Wird aufgerufen, wenn der Anwender die erweiterten Informationen der Programmzeitschrift abruft.
        this.onShowGuide = function (item, origin) {
        };
        // Wird aufgerufen, wenn der Anwender die Ausnahmen konfigurieren möchte.
        this.onException = function (item, origin) {
        };
        // Die zugehörigen Informationen der Programmzeitschrift.
        this.guideItem = new GuideItemCache();
        var me = this;

        // Zeiten umrechnen
        var duration = rawData.duration * 1000;
        var start = new Date(rawData.start);
        var end = new Date(start.getTime() + duration);

        // Daten aus der Rohdarstellung in das Modell kopieren
        me.station = (rawData.station == null) ? '(Aufzeichnung gelöscht)' : rawData.station;
        me.profile = (rawData.device == null) ? '' : rawData.device;
        me.displayStart = JMSLib.DateFormatter.getStartTime(start);
        me.displayEnd = JMSLib.DateFormatter.getEndTime(end);
        me.epgProfile = rawData.epgDevice;
        me.fullName = rawData.name;
        me.source = rawData.source;
        me.legacyId = rawData.id;
        me.start = start;
        me.end = end;

        // Aufzeichnungsoptionen
        me.currentGuide = rawData.epgCurrent ? '' : CSSClass.inactiveOptionClass;
        me.allAudio = rawData.allAudio ? '' : CSSClass.inactiveOptionClass;
        me.hasGuideEntry = rawData.epg ? '' : CSSClass.inactiveOptionClass;
        me.subTitles = rawData.dvbsub ? '' : CSSClass.inactiveOptionClass;
        me.videoText = rawData.ttx ? '' : CSSClass.inactiveOptionClass;
        me.dolby = rawData.ac3 ? '' : CSSClass.inactiveOptionClass;

        // Für Aufgaben konfigurieren wir keine Verweise
        if (me.station == 'PSI')
            return;
        if (me.station == 'EPG')
            return;

        // Aufzeichungsmodus ermitteln
        if (rawData.lost)
            me.mode = 'lost';
        else if (rawData.late)
            me.mode = 'late';
        else
            me.mode = 'intime';

        // Detailanzeige immer aktivieren
        me.showDetails = function () {
            me.onShowDetails(me, this);
        };
        me.detailsLink = 'javascript:void(0)';

        // Bearbeiten aktivieren
        if (me.legacyId != null)
            me.editLink = '#edit;id=' + me.legacyId;

        // Abruf der Programmzeitschrift vorbereiten
        if (rawData.epg) {
            me.showGuide = function () {
                me.onShowGuide(me, this);
            };
            me.guideLink = 'javascript:void(0)';
        }

        // Ausnahmen auswerten
        if (rawData.exception != null) {
            me.exceptionInfo = new PlanException(rawData.exception);

            me.exceptionMode = me.exceptionInfo.isEmpty() ? 'exceptOff' : 'exceptOn';
            me.showExceptions = function () {
                me.onException(me, this);
            };
        }

        // Die Endzeit könnte nicht wie gewünscht sein
        if (rawData.suspectEndTime)
            me.endTimeSuspect = CSSClass.badEndTime;
    }
    // Fordert die Informationen der Programmzeitschrift einmalig an und liefert das Ergebnis bei Folgeaufrufen.
    PlanEntry.prototype.requestGuide = function (dataAvailable) {
        this.guideItem.request(this.epgProfile, this.source, this.start, this.end, dataAvailable);
    };

    // Sendet die aktuelle Ausnahmeregel zur Änderung an den VCR.NET Recording Service.
    PlanEntry.prototype.updateException = function (onSuccess) {
        this.exceptionInfo.update(this.legacyId, onSuccess);
    };
    return PlanEntry;
})();

// Beschreibt einen einzelne Ausnahmeregel
var PlanException = (function () {
    function PlanException(rawData) {
        // Gesetzt, wenn diese Ausnahme aktiv ist.
        this.isActive = true;
        var me = this;

        // Daten aus den Rohdaten übernehmen
        me.referenceDayDisplay = parseInt(rawData.referenceDayDisplay, 10);
        me.originalStart = new Date(rawData.originalStart);
        me.originalDuration = rawData.originalDuration;
        me.referenceDay = rawData.referenceDay;
        me.durationDelta = rawData.timeDelta;
        me.startDelta = rawData.startShift;
        me.rawException = rawData;
    }
    // Meldet die aktuelle Startzeit.
    PlanException.prototype.start = function () {
        return new Date(this.originalStart.getTime() + 60 * this.startDelta * 1000);
    };

    // Meldet die aktuelle Endzeit.
    PlanException.prototype.end = function () {
        return new Date(this.start().getTime() + 60 * (this.originalDuration + this.durationDelta) * 1000);
    };

    // Prüft, ob eine Ausnahme definiert ist.
    PlanException.prototype.isEmpty = function () {
        return (this.startDelta == 0) && (this.durationDelta == 0);
    };

    // Meldet den Referenztag als Zeichenkette.
    PlanException.prototype.displayDay = function () {
        return JMSLib.DateFormatter.getStartDate(new Date(this.referenceDayDisplay));
    };

    // Meldet die aktuelle Startzeit als Zeichenkette.
    PlanException.prototype.displayStart = function () {
        return JMSLib.DateFormatter.getStartTime(this.start());
    };

    // Meldet die aktuelle Endzeit als Zeichenkette
    PlanException.prototype.displayEnd = function () {
        return JMSLib.DateFormatter.getEndTime(this.end());
    };

    // Meldet die aktuelle Dauer
    PlanException.prototype.displayDuration = function () {
        return this.originalDuration + this.durationDelta;
    };

    PlanException.prototype.isDurationDeltaValid = function () {
        return (this.durationDelta > -this.originalDuration);
    };

    // Sendet die aktuellen Daten der Ausnahme an den VCR.NET Recording Service.
    PlanException.prototype.update = function (planEntryLegacyId, onSuccess) {
        VCRServer.updateException(planEntryLegacyId, this.referenceDay, this.startDelta, this.durationDelta).done(onSuccess);
    };

    // Bereitet das Formular für Änderungen vor.
    PlanException.prototype.startEdit = function (item, element, reload) {
        // Wir wurden deaktiviert
        if (element == null)
            return;

        // Darstellung vorbereiten
        var durationSlider = $('#durationSlider');
        var disableButton = $('#disableButton');
        var startSlider = $('#startSlider');
        var zeroButton = $('#clearButton');
        var sendButton = $('#sendButton');
        var options = { min: -480, max: 480 };
        var me = this;

        // Konfigurieren
        durationSlider.slider(options);
        startSlider.slider(options);
        disableButton.button();
        zeroButton.button();
        sendButton.button();

        // Initialwerte setzen
        startSlider.slider('value', me.startDelta);
        durationSlider.slider('value', me.durationDelta);

        // Aktualisierung
        function refresh() {
            JMSLib.HTMLTemplate.applyTemplate(me, element);
            return true;
        }

        // Anmelden
        startSlider.slider({
            slide: function (slider, newValue) {
                me.startDelta = newValue.value;
                return refresh();
            }
        });
        durationSlider.slider({
            slide: function (slider, newValue) {
                // Das geht nicht
                if (me.originalDuration + newValue.value < 0)
                    return false;

                me.durationDelta = newValue.value;
                return refresh();
            }
        });
        zeroButton.click(function () {
            // Ausnahme entfernen
            durationSlider.slider('value', me.durationDelta = 0);
            startSlider.slider('value', me.startDelta = 0);

            refresh();
        });
        disableButton.click(function () {
            // Ausnahme entfernen
            durationSlider.slider('value', me.durationDelta = -me.originalDuration);

            refresh();
        });
        sendButton.click(function () {
            // Das geht nur einmal
            sendButton.button('disable', true);

            // Durchreichen
            item.updateException(reload);
        });
    };
    return PlanException;
})();

// Beschreibt die Daten einer Aufzeichnung
var ScheduleData = (function () {
    function ScheduleData(existingData) {
        // Die zugehörige Ausnahmeregel.
        this.exceptionInfos = new Array();
        var me = this;

        // Schauen wir mal, ob wir geladen wurden
        if (existingData != null) {
            // Aufzeichnungsdaten prüfen
            var rawData = existingData.schedule;
            if (rawData != null) {
                var repeat = rawData.repeatPattern;
                var start = new Date(rawData.firstStart);
                var end = new Date(start.getTime() + 60000 * rawData.duration);

                // Übernehmen
                me.exceptionInfos = $.map(rawData.exceptions, function (rawException) {
                    return new PlanException(rawException);
                });
                me.lastDay = (repeat == 0) ? ScheduleData.maximumDate : new Date(rawData.lastDay);
                me.firstStart = new Date(start.getFullYear(), start.getMonth(), start.getDate());
                me.repeatWednesday = (repeat & ScheduleData.flagWednesday) != 0;
                me.repeatThursday = (repeat & ScheduleData.flagThursday) != 0;
                me.repeatSaturday = (repeat & ScheduleData.flagSaturday) != 0;
                me.repeatTuesday = (repeat & ScheduleData.flagTuesday) != 0;
                me.repeatMonday = (repeat & ScheduleData.flagMonday) != 0;
                me.repeatFriday = (repeat & ScheduleData.flagFriday) != 0;
                me.repeatSunday = (repeat & ScheduleData.flagSunday) != 0;
                me.startTime = JMSLib.DateFormatter.getEndTime(start);
                me.endTime = JMSLib.DateFormatter.getEndTime(end);
                me.withSubtitles = rawData.withSubtitles;
                me.withVideotext = rawData.withVideotext;
                me.allLanguages = rawData.allLanguages;
                me.includeDolby = rawData.includeDolby;
                me.sourceName = rawData.sourceName, me.id = existingData.scheduleId;
                me.name = rawData.name;

                // Fertig
                return;
            }
        }

        var now = Date.now();
        var nowDate = new Date(now);

        // Eine ganz neue Aufzeichnung.
        me.firstStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate());
        me.endTime = JMSLib.DateFormatter.getEndTime(new Date(now + 7200000));
        me.withSubtitles = VCRServer.UserProfile.global.defaultDVBSubtitles;
        me.allLanguages = VCRServer.UserProfile.global.defaultAllLanguages;
        me.withVideotext = VCRServer.UserProfile.global.defaultVideotext;
        me.includeDolby = VCRServer.UserProfile.global.defaultDolby;
        me.startTime = JMSLib.DateFormatter.getEndTime(nowDate);
        me.lastDay = ScheduleData.maximumDate;
        me.repeatWednesday = false;
        me.repeatThursday = false;
        me.repeatSaturday = false;
        me.repeatTuesday = false;
        me.repeatMonday = false;
        me.repeatFriday = false;
        me.repeatSunday = false;
        me.sourceName = '';
        me.name = '';
    }
    // Meldet das Wiederholungsmuster.
    ScheduleData.prototype.repeatPattern = function () {
        var pattern = (this.repeatMonday ? ScheduleData.flagMonday : 0) | (this.repeatTuesday ? ScheduleData.flagTuesday : 0) | (this.repeatWednesday ? ScheduleData.flagWednesday : 0) | (this.repeatThursday ? ScheduleData.flagThursday : 0) | (this.repeatFriday ? ScheduleData.flagFriday : 0) | (this.repeatSaturday ? ScheduleData.flagSaturday : 0) | (this.repeatSunday ? ScheduleData.flagSunday : 0);

        return pattern;
    };

    // Erstellt eine für die Datenübertragung geeignete Variante.
    ScheduleData.prototype.toWebService = function () {
        // Ein bißchen herumrechnen, um die Zeiten zu bekommen
        var startTime = JMSLib.DateFormatter.parseTime(this.startTime);
        var endTime = JMSLib.DateFormatter.parseTime(this.endTime);

        // Wir müssen sicherstellen, dass uns die Umstellung zwischen Sommer- und Winterzeit keinen Streich spielt
        var firstYear = this.firstStart.getFullYear();
        var firstMonth = this.firstStart.getMonth();
        var firstDay = this.firstStart.getDate();
        var fullStart = new Date(firstYear, firstMonth, firstDay, Math.floor(startTime / 3600000), (startTime / 60000) % 60);
        var fullEnd = new Date(firstYear, firstMonth, firstDay, Math.floor(endTime / 3600000), (endTime / 60000) % 60);

        if (startTime >= endTime)
            fullEnd.setDate(firstDay + 1);

        var duration = fullEnd.getTime() - fullStart.getTime();

        var localEnd = this.lastDay;
        if (localEnd == null)
            localEnd = new Date(2999, 11, 31);
        var utcEnd = new Date(Date.UTC(localEnd.getFullYear(), localEnd.getMonth(), localEnd.getDate()));

        // Nun noch die verbleibenden Ausnahmen einrichten
        var exceptions = new Array();

        $.each(this.exceptionInfos, function (index, info) {
            if (info.isActive)
                exceptions.push(info.rawException);
        });

        // Fertig
        var contract = {
            firstStart: fullStart.toISOString(),
            repeatPattern: this.repeatPattern(),
            withVideotext: this.withVideotext,
            withSubtitles: this.withSubtitles,
            allLanguages: this.allLanguages,
            includeDolby: this.includeDolby,
            lastDay: utcEnd.toISOString(),
            sourceName: this.sourceName,
            duration: duration / 60000,
            exceptions: exceptions,
            name: this.name
        };

        // Report
        return contract;
    };
    ScheduleData.minimumDate = new Date(1963, 8, 29);

    ScheduleData.maximumDate = new Date(2999, 11, 31);

    ScheduleData.flagMonday = 0x01;

    ScheduleData.flagTuesday = 0x02;

    ScheduleData.flagWednesday = 0x04;

    ScheduleData.flagThursday = 0x08;

    ScheduleData.flagFriday = 0x10;

    ScheduleData.flagSaturday = 0x20;

    ScheduleData.flagSunday = 0x40;
    return ScheduleData;
})();

// Algorithmen zur Prüfung der Eingaben einer Aufzeichnung
var ScheduleDataValidator = (function () {
    function ScheduleDataValidator(job, existingData) {
        this.model = new ScheduleData(existingData);
        this.job = job;

        job.schedules.push(this);
    }
    ScheduleDataValidator.prototype.validate = function () {
        var me = this;

        // Das sollen wir prüfen
        var schedule = me.model;
        var job = me.job.model;

        // Alles zurücksetzen
        me.firstStart = null;
        me.startTime = null;
        me.lastDay = null;
        me.endTime = null;
        me.name = null;

        // Name
        schedule.name = schedule.name.trim();
        if (schedule.name.length > 0)
            if (!JobDataValidator.isNameValid(schedule.name))
                this.name = 'Der Name der Aufzeichnung enthält ungültige Zeichen';

        // Zeiten
        var startTime = JMSLib.DateFormatter.parseTime(schedule.startTime);
        if (startTime == null)
            this.startTime = 'Ungültige Uhrzeit';
        var endTime = JMSLib.DateFormatter.parseTime(schedule.endTime);
        if (endTime == null)
            this.endTime = 'Ungültige Uhrzeit';
        if (startTime != null)
            if (endTime != null)
                if (startTime == endTime)
                    this.endTime = 'Die Aufzeichnungsdauer muss mindestens eine Minute betragen';

        // Tage
        var firstDay = schedule.firstStart;
        if (firstDay == null)
            this.firstStart = 'Es muss ein Datum für die erste Aufzeichnung angegeben werden';
        else if (firstDay < ScheduleData.minimumDate)
            this.firstStart = 'Das Datum für die erste Aufzeichnung ist ungültig';

        // Sichtbarkeit prüfen
        var endDate = me.view.find('#endDateContainer');
        if (schedule.repeatPattern() == 0) {
            endDate.addClass(JMSLib.CSSClass.invisible);
        } else {
            endDate.removeClass(JMSLib.CSSClass.invisible);

            var lastDay = schedule.lastDay;
            if (lastDay == null)
                this.lastDay = 'Es muss ein Datum für die letzte Aufzeichnung angegeben werden';
            else if (firstDay != null)
                if (lastDay < firstDay)
                    this.lastDay = 'Das Datum der letzten Aufzeichnung darf nicht vor der ersten Aufzeichnung liegen';
        }

        // Der Auftrag muss auch geprüft werden
        if (!JMSLib.Bindings.validate(me.job))
            return;

        // Ergebnis berechnen
        var isValid = (me.firstStart == null) && (me.startTime == null) && (me.lastDay == null) && (me.endTime == null) && (me.name == null);

        // Schaltfläche deaktivieren
        if (!isValid)
            me.job.sendButton.button('option', 'disabled', true);
    };
    return ScheduleDataValidator;
})();

// Beschreibt die Daten eines Auftrags
var JobData = (function () {
    function JobData(existingData, defaultProfile) {
        var me = this;

        // Schauen wir mal, ob wir etwas ändern sollen
        if (existingData != null) {
            // Auftragsdaten müssen vorhanden sein
            var rawData = existingData.job;
            if (rawData != null) {
                // Da gibt es schon etwas für uns vorbereitetes
                me.lockedToDevice = rawData.lockedToDevice;
                me.withSubtitles = rawData.withSubtitles;
                me.withVideotext = rawData.withVideotext;
                me.allLanguages = rawData.allLanguages;
                me.includeDolby = rawData.includeDolby;
                me.sourceName = rawData.sourceName;
                me.directory = rawData.directory;
                me.operationType = 'Übernehmen';
                me.id = existingData.jobId;
                me.device = rawData.device;
                me.name = rawData.name;

                return;
            }
        }

        // Ein ganz neuer Auftrag
        me.withSubtitles = VCRServer.UserProfile.global.defaultDVBSubtitles;
        me.allLanguages = VCRServer.UserProfile.global.defaultAllLanguages;
        me.withVideotext = VCRServer.UserProfile.global.defaultVideotext;
        me.includeDolby = VCRServer.UserProfile.global.defaultDolby;
        me.operationType = 'Anlegen';
        me.device = defaultProfile;
        me.lockedToDevice = false;
        me.sourceName = '';
        me.directory = '';
        me.name = '';
        me.id = null;
    }
    // Erstellt eine für die Datenübertragung geeignete Variante.
    JobData.prototype.toWebService = function () {
        var contract = {
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
    };

    // Aktualisiert die Daten des Auftrags und der zugehörigen Aufzeichnung.
    JobData.prototype.createOrUpdate = function (schedule, onError) {
        // Wohin geht es nach dem Abschluss
        var whenFinished;
        if (this.guideAfterAdd)
            whenFinished = 'guide';
        else
            whenFinished = 'plan';

        var data = new JobScheduleDataContract(this.toWebService(), schedule.toWebService());

        // Aktualisierungsbefehl auslösen
        data.update(this.id, schedule.id, whenFinished, onError);
    };
    return JobData;
})();

// Algorithmen zur Prüfung der Eingaben eines Auftrags
var JobDataValidator = (function () {
    function JobDataValidator(existingData, updateButton, defaultProfile) {
        // Gesetzt, während eine Anfrage an der Web Dienst läuft.
        this.waiting = false;
        // Alle zurzeit in Bearbeitung befindlichen Aufzeichnungen dieses Auftrags.
        this.schedules = new Array();
        var me = this;

        me.model = new JobData(existingData, defaultProfile);
        me.sendButton = updateButton;

        // Schaltfläche aufbereiten
        updateButton.text(me.model.operationType);
        updateButton.button({ disabled: true });
        updateButton.click(function () {
            updateButton.button('option', 'disabled', true);

            // Das ist die eigentliche Aktualisierung
            me.model.createOrUpdate(me.schedules[0].model, function (error) {
                JMSLib.Bindings.setErrorIndicator(updateButton, error);

                me.waiting = false;
            });
        });
    }
    JobDataValidator.isNameValid = function (name) {
        return name.search(JobDataValidator.forbidenCharacters) < 0;
    };

    JobDataValidator.prototype.validate = function () {
        var me = this;

        // Das sollen wir prüfen
        var job = me.model;

        // Alles zurücksetzen
        JMSLib.Bindings.setErrorIndicator(me.sendButton, null);
        me.sourceName = null;
        me.name = null;

        // Name
        job.name = job.name.trim();
        if (job.name.length < 1)
            this.name = 'Ein Auftrag muss immer einen Namen haben';
        else if (!JobDataValidator.isNameValid(job.name))
            this.name = 'Der Name des Auftrags enthält ungültige Zeichen';

        // Quelle
        if (job.sourceName.length < 1)
            $.each(me.schedules, function (index, schedule) {
                var sourceName = schedule.model.sourceName;
                if (sourceName != null)
                    if (sourceName.length > 0)
                        return true;

                me.sourceName = 'Es muss immer eine Quelle angegeben werden (auf Wunsch auch bei der Aufzeichnung)';

                return false;
            });

        // Ergebnis berechnen
        var isValid = (me.sourceName == null) && (me.name == null);

        // Schaltfläche entsprechend einstellen
        me.sendButton.button('option', 'disabled', me.waiting || !isValid);
    };

    // Bereitet das Löschen der Aufzeichnung vor - das Löschen der letzten Aufzeichnung eines Auftrags entfernt auch den Auftrag.
    JobDataValidator.prototype.prepareDelete = function (info) {
        var job = this.model;
        var schedule = this.schedules[0].model;

        var deleteButton = $(info.currentTarget);
        deleteButton.addClass(CSSClass.danger);
        deleteButton.unbind('click');

        deleteButton.click(function () {
            deleteButton.button('option', 'disabled', true);

            VCRServer.deleteSchedule(job.id, schedule.id).done(function () {
                window.location.href = '#plan';
            }).fail(function (result) {
                var info = $.parseJSON(result.responseText);

                deleteButton.addClass(JMSLib.CSSClass.invalid);
                deleteButton.attr('title', info.ExceptionMessage);
                deleteButton.button('option', 'disabled', false);
            });
        });
    };
    JobDataValidator.forbidenCharacters = /[\\\/\:\*\?\"\<\>\|]/;
    return JobDataValidator;
})();

// Eingabeelemente für die Sicherheitskonfiguration
var SecuritySettingsValidator = (function () {
    function SecuritySettingsValidator(settings) {
        this.model = settings;
    }
    // Methode zur Durchführung einer Prüfung
    SecuritySettingsValidator.prototype.validate = function () {
    };
    return SecuritySettingsValidator;
})();

// Eingabeelemente für das Regelwerk der Aufzeichnungsplanung
var ScheduleRulesValidator = (function () {
    function ScheduleRulesValidator(settings) {
        this.model = settings;
    }
    // Methode zur Durchführung einer Prüfung
    ScheduleRulesValidator.prototype.validate = function () {
    };
    return ScheduleRulesValidator;
})();

// Eingabeelemente für die Verzeichnisse
var DirectorySettingsValidator = (function () {
    function DirectorySettingsValidator(settings, send) {
        this.sendButton = send;
        this.model = settings;
    }
    // Methode zur Durchführung einer Prüfung
    DirectorySettingsValidator.prototype.validate = function () {
        // Wir prüfen hier nur das Suchmuster
        var pattern = this.model.pattern;
        if ((pattern != null) && (pattern.length > 0))
            this.pattern = null;
        else
            this.pattern = 'Das Muster für Dateinamen darf nicht leer sein.';

        this.sendButton.button('option', 'disabled', this.pattern != null);
    };
    return DirectorySettingsValidator;
})();

// Eingabeelemente für die Programmzeitschrift
var GuideSettingsValidator = (function () {
    function GuideSettingsValidator(settings, send, form) {
        this.model = settings;
        this.sendButton = send;
        this.editForm = form;

        // Stundenliste in Markierungen umsetzen
        JMSLib.HourListSettings.decompress(settings, settings.hours);

        // Aktivierung berücksichtigen
        GuideSettingsValidator.activated(settings, settings.duration > 0);
    }
    // Liest oder setzt den Aktivierungszustand
    GuideSettingsValidator.activated = function (settings, newValue) {
        if (typeof newValue === "undefined") { newValue = null; }
        if (newValue != null)
            settings['activated'] = newValue;
        else
            return settings['activated'];
    };

    // Stellt sicher, dass nur bekannte Eigenschaften an den Dienst übertragen werden
    GuideSettingsValidator.filterPropertiesOnSend = function (key, value) {
        if (key == 'sourceName')
            return undefined;
        if (key == 'activated')
            return undefined;
        if (JMSLib.HourListSettings.isHourFlag(key))
            return undefined;

        return value;
    };

    // Methode zur Durchführung einer Prüfung
    GuideSettingsValidator.prototype.validate = function () {
        var model = this.model;

        // Anzeige vorbereiten
        var isActive = GuideSettingsValidator.activated(model);
        if (isActive)
            this.editForm.removeClass(JMSLib.CSSClass.invisible);
        else
            this.editForm.addClass(JMSLib.CSSClass.invisible);

        // Zahlwerte prüfen
        this.duration = JMSLib.Bindings.checkNumber(model.duration, 5, 55);
        if ((model.joinHours != null) && (model.joinHours.length > 0))
            this.joinHours = JMSLib.Bindings.checkNumber(model.joinHours, 1, 23);
        else
            this.joinHours = null;
        if ((model.minDelay != null) && (model.minDelay.length > 0))
            this.minDelay = JMSLib.Bindings.checkNumber(model.minDelay, 1, 23);
        else
            this.minDelay = null;

        // Stundenmuster zurückmischen
        model.hours = JMSLib.HourListSettings.compress(model);

        // Ergebnis der Prüfungen auswerten
        var isValid = !isActive || ((this.duration == null) && (this.joinHours == null) && (this.minDelay == null));

        this.sendButton.button('option', 'disabled', !isValid);
    };
    return GuideSettingsValidator;
})();

// Eingabeelemente für die Aktualisierung der Quellen
var SourceScanSettingsValidator = (function () {
    function SourceScanSettingsValidator(settings, send) {
        this.model = settings;
        this.sendButton = send;

        // Stundenliste in Markierungen umsetzen
        JMSLib.HourListSettings.decompress(settings, settings.hours);

        // Modus setzen
        if (settings.interval == null)
            SourceScanSettingsValidator.mode(settings, 'D');
        else if (settings.interval < 0)
            SourceScanSettingsValidator.mode(settings, 'M');
        else
            SourceScanSettingsValidator.mode(settings, 'P');
    }
    // Liest oder setzt den Aktivierungszustand
    SourceScanSettingsValidator.mode = function (settings, newValue) {
        if (typeof newValue === "undefined") { newValue = null; }
        if (newValue != null)
            settings['mode'] = newValue;
        else
            return settings['mode'];
    };

    // Stellt sicher, dass nur bekannte Eigenschaften an den Dienst übertragen werden
    SourceScanSettingsValidator.filterPropertiesOnSend = function (key, value) {
        if (key == 'mode')
            return undefined;
        if (JMSLib.HourListSettings.isHourFlag(key))
            return undefined;

        return value;
    };

    // Methode zur Durchführung einer Prüfung
    SourceScanSettingsValidator.prototype.validate = function () {
        var model = this.model;

        // Anzeige anpassen
        var mode = SourceScanSettingsValidator.mode(model);
        switch (mode) {
            case 'D':
                $('#scanFormPeriodic').addClass(JMSLib.CSSClass.invisible);
                $('#scanFormDuration').addClass(JMSLib.CSSClass.invisible);
                break;
            case 'M':
                $('#scanFormPeriodic').addClass(JMSLib.CSSClass.invisible);
                $('#scanFormDuration').removeClass(JMSLib.CSSClass.invisible);
                break;
            case 'P':
                $('#scanFormPeriodic').removeClass(JMSLib.CSSClass.invisible);
                $('#scanFormDuration').removeClass(JMSLib.CSSClass.invisible);
                break;
        }

        // Zahlwerte prüfen
        this.duration = JMSLib.Bindings.checkNumber(model.duration, 5, 55);
        this.interval = JMSLib.Bindings.checkNumber(model.interval, 1, 28);
        if ((model.joinDays != null) && (model.joinDays.length > 0))
            this.joinDays = JMSLib.Bindings.checkNumber(model.joinDays, 1, 14);
        else
            this.joinDays = null;

        // Stundenmuster zurückmischen
        model.hours = JMSLib.HourListSettings.compress(model);

        // Ergebnis der Prüfungen auswerten
        var isValid = true;
        if (mode == 'P')
            isValid = (this.duration == null) && (this.joinDays == null) && (this.interval == null);
        else if (mode == 'M')
            isValid = (this.duration == null);

        this.sendButton.button('option', 'disabled', !isValid);
    };
    return SourceScanSettingsValidator;
})();

// Eingabeelemente für ein einzelnes Gerät
var ProfileValidator = (function () {
    function ProfileValidator(settings, listValidator) {
        this.model = settings;
        this.list = listValidator;
    }
    // Methode zur Durchführung einer Prüfung
    ProfileValidator.prototype.validate = function () {
        this.list.validate();
    };

    // Methode zur Durchführung einer Prüfung
    ProfileValidator.prototype.doValidate = function (defaultProfile) {
        var model = this.model;

        this.sourceLimit = JMSLib.Bindings.checkNumber(model.sourceLimit, 1, 32);
        this.priority = JMSLib.Bindings.checkNumber(model.priority, 0, 100);
        this.ciLimit = JMSLib.Bindings.checkNumber(model.ciLimit, 0, 16);
        this.active = null;

        if (model.name == defaultProfile)
            if (!model.active)
                this.active = 'Das bevorzugte Geräteprofil muss auch für Aufzeichnungen verwendet werden.';

        JMSLib.Bindings.synchronizeErrors(this);

        return (this.active == null) && (this.priority == null) && (this.ciLimit == null) && (this.sourceLimit == null);
    };
    return ProfileValidator;
})();

// Eingabeelemente für die Geräte
var ProfileSettingsValidator = (function () {
    function ProfileSettingsValidator(settings, send) {
        // Alle untergeordneten Prüfalgorithmen
        this.profiles = new Array();
        this.model = settings;
        this.sendButton = send;
    }
    // Erzeugt die Präsentation für ein einzelnes Geräteprofil
    ProfileSettingsValidator.prototype.bindProfiles = function (deviceTable, deviceTemplate) {
        var me = this;

        $.each(this.model.profiles, function (index, profile) {
            var view = deviceTemplate.clone();
            var validator = new ProfileValidator(profile, me);

            me.profiles.push(validator);
            deviceTable.append(view);

            JMSLib.Bindings.bind(validator, view);
        });
    };

    // Methode zur Durchführung einer Prüfung
    ProfileSettingsValidator.prototype.validate = function () {
        var me = this;
        var model = me.model;
        var defaultProfile = model.defaultProfile;
        var isValid = true;

        me.defaultProfile = null;

        // Alle Geräte individuell überprüfen
        $.each(me.profiles, function (index, profile) {
            if (profile.doValidate(defaultProfile))
                return;

            isValid = false;

            if (profile.active != null)
                me.defaultProfile = 'Dieses Gerät ist nicht für Aufzeichnungen vorgesehen';
        });

        JMSLib.Bindings.synchronizeErrors(this);

        me.sendButton.button('option', 'disabled', !isValid);
    };
    return ProfileSettingsValidator;
})();

// Eingabeelemente für die Sicherheitskonfiguration
var OtherSettingsValidator = (function () {
    function OtherSettingsValidator(settings, send) {
        this.model = settings;
        this.sendButton = send;

        if (settings.mayHibernate)
            if (settings.useStandBy)
                OtherSettingsValidator.hibernationMode(settings, 'S3');
            else
                OtherSettingsValidator.hibernationMode(settings, 'S4');
        else
            OtherSettingsValidator.hibernationMode(settings, 'OFF');
    }
    // Liest oder setzt die Art des Übergangs in den Schlafzustand
    OtherSettingsValidator.hibernationMode = function (settings, newValue) {
        if (typeof newValue === "undefined") { newValue = null; }
        if (newValue != null)
            settings['hibernate'] = newValue;
        else
            return settings['hibernate'];
    };

    // Methode zur Durchführung einer Prüfung
    OtherSettingsValidator.prototype.validate = function () {
        var model = this.model;

        this.forcedHibernationDelay = JMSLib.Bindings.checkNumber(model.forcedHibernationDelay, 5, 60);
        this.hibernationDelay = JMSLib.Bindings.checkNumber(model.hibernationDelay, 0, 600);
        this.sslPort = JMSLib.Bindings.checkNumber(model.sslPort, 1, 0xffff);
        this.webPort = JMSLib.Bindings.checkNumber(model.webPort, 1, 0xffff);
        this.protocol = JMSLib.Bindings.checkNumber(model.protocol, 1, 13);
        this.archive = JMSLib.Bindings.checkNumber(model.archive, 1, 13);

        var isValid = (this.forcedHibernationDelay == null) && (this.hibernationDelay == null) && (this.protocol == null) && (this.sslPort == null) && (this.webPort == null) && (this.archive == null);

        this.sendButton.button('option', 'disabled', !isValid);
    };
    return OtherSettingsValidator;
})();

// Das Modell zur Anzeige einer Aktivität auf einem Gerät
var CurrentInfo = (function () {
    function CurrentInfo(rawData) {
        // Aktiv, wenn die Aufzeichnung verändert werden kann.
        this.editLink = null;
        // Die CSS Klasse zum Verbergen der Verweise zur Ansicht einer Aufzeichnung.
        this.hideViewer = JMSLib.CSSClass.invisible;
        // Die zugehörigen Informationen der Programmzeitschrift.
        this.guideItem = new GuideItemCache();
        var me = this;

        // Sonderbehandlung für unbenutzte Geräte
        if (rawData.isIdle) {
            // Eigentlich zeigen wir nichts
            me.hideTarget = JMSLib.CSSClass.invisible;
            me.name = '(keine Aufzeichnung geplant)';
            me.device = rawData.device;
            me.displayStart = '';
            me.displayEnd = '';
            me.mode = 'intime';
            me.size = '';

            // Fertig
            return;
        }

        // Zeiten umrechnen
        var duration = rawData.duration * 1000;
        var start = new Date(rawData.start);
        var end = new Date(start.getTime() + duration);
        var outdated = end.getTime() <= Date.now();

        // Übernehmen
        me.displayStart = JMSLib.DateFormatter.getStartTime(start);
        me.originalRemainingMinutes = rawData.remainingMinutes;
        me.size = (rawData.size == null) ? '' : rawData.size;
        me.displayEnd = JMSLib.DateFormatter.getEndTime(end);
        me.scheduleIdentifier = rawData.referenceId;
        me.streamTarget = rawData.streamTarget;
        me.device = rawData.device;
        me.source = rawData.source;
        me.legacyId = rawData.id;
        me.name = rawData.name;
        me.start = start;
        me.end = end;

        // Aufzeichungsmodus ermitteln
        if (me.scheduleIdentifier != null)
            if (outdated)
                me.mode = 'null';
            else
                me.mode = 'running';
        else if (rawData.late)
            me.mode = 'late';
        else
            me.mode = 'intime';

        // Bearbeiten aktivieren
        if (me.legacyId != null)
            me.editLink = '#edit;id=' + me.legacyId;

        // Abruf der Programmzeitschrift vorbereiten
        if (!outdated)
            if (rawData.epg) {
                me.showGuide = function () {
                    CurrentInfo.guideDisplay(me, this);
                };
                me.guideLink = 'javascript:void(0)';
            }

        // Manipulation laufender Aufzeichnungen
        if (me.scheduleIdentifier != null) {
            me.editActive = function () {
                CurrentInfo.startAbort(me, this);
            };
            me.activeLink = 'javascript:void(0)';

            if (rawData.streamIndex >= 0) {
                var url = VCRServer.getDeviceRoot() + encodeURIComponent(me.device) + '/' + rawData.streamIndex + '/';

                me.viewShift = url + 'TimeShift';
                me.viewLive = url + 'Live';
                me.hideViewer = null;
            }
        }

        // Zieladresse ausblenden
        if (me.streamTarget == null)
            me.hideTarget = JMSLib.CSSClass.invisible;
    }
    // Der aktuelle Endzeitpunkt der Aufzeichnung, so wie vom Anwender korrigiert.
    CurrentInfo.prototype.currentEnd = function () {
        return new Date(this.end.getTime() - (this.originalRemainingMinutes - this.remainingMinutes) * 60000);
    };

    // Der Anzeigetext für den aktuellen Endzeitpunkt.
    CurrentInfo.prototype.currentEndDisplay = function () {
        return JMSLib.DateFormatter.getEndTime(this.currentEnd());
    };

    // Fordert die Informationen der Programmzeitschrift einmalig an und liefert das Ergebnis bei Folgeaufrufen.
    CurrentInfo.prototype.requestGuide = function (dataAvailable) {
        this.guideItem.request(this.device, this.source, this.start, this.end, dataAvailable);
    };

    // Ruft die aktuelle Liste der Aufzeichnungen vom Web Dienst ab.
    CurrentInfo.load = function (whenLoaded) {
        VCRServer.getPlanCurrent().done(function (data) {
            whenLoaded($.map(data, function (rawData) {
                return new CurrentInfo(rawData);
            }));
        });
    };

    // Aktualisiert den Endzeitpunkt dieser laufenden Aufzeichnung.
    CurrentInfo.prototype.updateEndTime = function (whenDone) {
        var end = (this.remainingMinutes > 0) ? this.currentEnd() : this.start;

        VCRServer.updateEndTime(this.device, this.suppressHibernate, this.scheduleIdentifier, end).done(whenDone);
    };
    return CurrentInfo;
})();


// Stelle die Daten einer Aufzeichnung dar
var InfoSchedule = (function () {
    function InfoSchedule(rawInfo, isActive) {
        // Der Name des Auftrags ist immer leer
        this.jobText = '';
        // Dargestellt wird der leere Name als weiße Fläche
        this.jobTextClass = CSSClass.noJobText;
        var me = this;

        // Name von Aufzeichnung und Quelle ermitteln
        var name = (rawInfo.name == '') ? 'Aufzeichnung' : rawInfo.name;
        var source = rawInfo.sourceName;

        // Zeiten ermitteln
        var start = new Date(rawInfo.start);
        var pattern = rawInfo.repeatPattern;
        var startAsString;

        // Startzeitpunkt und Wiederholungsmuster berechnen
        if (pattern == 0)
            startAsString = JMSLib.DateFormatter.getStartTime(start);
        else {
            var merged = '';
            if ((pattern & ScheduleData.flagMonday) != 0)
                merged += 'Mo';
            if ((pattern & ScheduleData.flagTuesday) != 0)
                merged += 'Di';
            if ((pattern & ScheduleData.flagWednesday) != 0)
                merged += 'Mi';
            if ((pattern & ScheduleData.flagThursday) != 0)
                merged += 'Do';
            if ((pattern & ScheduleData.flagFriday) != 0)
                merged += 'Fr';
            if ((pattern & ScheduleData.flagSaturday) != 0)
                merged += 'Sa';
            if ((pattern & ScheduleData.flagSunday) != 0)
                merged += 'So';

            startAsString = merged + ' ' + JMSLib.DateFormatter.getEndTime(start);
        }

        me.scheduleText = name + ': ' + startAsString + ' auf ' + source;
        me.isActive = isActive;
        me.link = rawInfo.id;
    }
    return InfoSchedule;
})();

// Bietet das Anlegen einer neuen Aufzeichnung zu einem Auftrag an
var InfoNew = (function () {
    function InfoNew(legacyId, isActive) {
        // Der Name des Auftrags ist immer leer
        this.jobText = '';
        // Dargestellt wird der leere Name als weiße Fläche
        this.jobTextClass = CSSClass.noJobText;
        // Der besondere Name der Aufzeichnung
        this.scheduleText = '(Neue Aufzeichnung)';
        this.link = legacyId;
        this.isActive = isActive;
    }
    return InfoNew;
})();

// Zeigt einen Auftrag als Knoten der Darstellung an
var InfoJob = (function () {
    function InfoJob(rawInfo) {
        // Die Darstellung des Namens als Knoten im Baum
        this.jobTextClass = CSSClass.jobText;
        // Der Name der Aufzeichnung ist leer
        this.scheduleText = '';
        // Der Auftrag bietet keinen Verweis zur Pflege an
        this.link = '';
        var me = this;

        me.isActive = rawInfo.active;
        me.jobText = rawInfo.name;
        me.legacyId = rawInfo.id;
    }
    // Aktualisiert die Liste der Aufträge
    InfoJob.load = function (whenLoaded) {
        VCRServer.getInfoJobs().done(function (data) {
            // Wandeln
            var rows = new Array();

            // Alles auslesen
            $.each(data, function (index, rawInfo) {
                var job = new InfoJob(rawInfo);

                // Einträge für den Auftrag als Knoten und das Erzeugen einer neuen Aufzeichnung zum Auftrag anlegen
                rows.push(job);
                rows.push(new InfoNew(job.legacyId, job.isActive));

                // Für jede existierende Aufzeichnung einen Eintrag anlegen
                $.map(rawInfo.schedules, function (schedule) {
                    rows.push(new InfoSchedule(schedule, job.isActive));
                });
            });

            // Aktivieren
            whenLoaded(rows);
        });
    };
    return InfoJob;
})();

// Ein einzelner Protokolleintrag
var ProtocolEntry = (function () {
    function ProtocolEntry(rawEntry) {
        // Der Startzeitpunkt in der üblichen Kurzanzeige
        this.displayStart = null;
        // Alle Dateien
        this.files = new Array();
        // Die primäre Datei
        this.primaryFile = null;
        if ((rawEntry.start != null) && (rawEntry.start.length > 0)) {
            var start = new Date(rawEntry.start);
            this.displayStart = JMSLib.DateFormatter.getStartTime(start);
        }

        var end = new Date(rawEntry.end);
        this.displayEnd = JMSLib.DateFormatter.getEndTime(end);
        this.fullEnd = JMSLib.DateFormatter.getStartTime(end);

        this.primaryFile = rawEntry.primaryFile;
        this.source = rawEntry.firstSourceName;
        this.sizeHint = rawEntry.size;

        switch (this.source) {
            case 'EPG':
                this.displaySource = 'Programmzeitschrift';
                break;
            case 'PSI':
                this.displaySource = 'Sendersuchlauf';
                break;
            case 'LIVE':
                this.displaySource = 'Zapping';
                break;
            default:
                this.displaySource = this.source;
                break;
        }

        this.files = $.map(rawEntry.files, function (file) {
            return VCRServer.getFileRoot() + encodeURIComponent(file);
        });
    }
    return ProtocolEntry;
})();

////////////////////////// Die einzelnen Seiten //////////////////////////
// Benutzerspezifische Einstellungen
var settingsPage = (function (_super) {
    __extends(settingsPage, _super);
    function settingsPage() {
        _super.apply(this, arguments);
        this.title = 'Individuelle Einstellungen ändern';
        this.visibleLinks = '.guideLink, .newLink, .planLink, .currentLink';
    }
    settingsPage.prototype.onInitialize = function () {
        VCRServer.UserProfile.global.register(this.registerAsyncCall());
    };

    settingsPage.prototype.onShow = function () {
        JMSLib.Bindings.bind(new UserSettingsValidator($('#settingsUpdateButton')), $('#settingsData'));
    };
    return settingsPage;
})(Page);

// Fragen und Antworten
var faqPage = (function (_super) {
    __extends(faqPage, _super);
    function faqPage() {
        _super.apply(this, arguments);
        this.title = '';
        this.visibleLinks = '.guideLink, .newLink, .planLink, .currentLink';
    }
    faqPage.prototype.onInitialize = function () {
        var me = this;
        var templateAvailable = me.registerAsyncCall();

        // Name der gewünschten Hilfeseite ermitteln
        var hash = window.location.hash;
        var index = hash.indexOf(';');
        var faq = hash.substr(index + 1);

        // Hilfeseite laden
        JMSLib.TemplateLoader.loadAbsolute('faq/' + faq + '.html').done(function (template) {
            var content = $(template).find('#faqContents');
            var target = $('#faqContent');

            // Hilfseite einmischen
            target.html(null);
            target.html(content.html());
            target.addClass(CSSClass.faq);

            // Und dann nur noch die Überschrift merken
            me.title = content.attr('data-title');

            templateAvailable();
        });
    };
    return faqPage;
})(Page);

// Die Startseite
var homePage = (function (_super) {
    __extends(homePage, _super);
    function homePage() {
        _super.apply(this, arguments);
        this.title = '';
        this.visibleLinks = '';
    }
    // Aktiviert eine Anzeige zur Aktualisierung
    homePage.prototype.showUpdate = function (button, index, method) {
        this.startUpdate = function () {
            VCRServer.triggerTask(method).done(function () {
                window.location.hash = 'current';
            });
        };

        // Anzeigen oder ausblenden
        var view = this.detailsManager.toggle(this, button[0], index);
        if (view == null)
            return;

        // Aufbereiten
        view.find('.editButtons').button();
    };

    // Prüft, ob eine neue Version vorliegt.
    homePage.prototype.checkUpdate = function (button) {
        // Anzeigen oder ausblenden
        var view = this.detailsManager.toggle(this, button[0], 2);
        if (view == null)
            return;

        var versionInfo = { currentVersion: this.serverInfo.msiVersion, onlineVersion: '(wird gesucht)', hintClass: '' };

        // In die Oberfläche einblenden
        JMSLib.HTMLTemplate.applyTemplate(versionInfo, view);

        // Und vom Server abfragen
        var xhr = new XMLHttpRequest();
        xhr.open('GET', 'http://downloads.psimarron.net');
        xhr.onreadystatechange = function () {
            if (xhr.readyState != 4)
                return;
            if (xhr.status != 200)
                return;

            var html = xhr.responseText;
            var match = homePage.versionExtract.exec(html);
            if (match == null)
                return;
            if (match.length < 2)
                return;

            // Download Version übernehmen und eventuell markieren
            versionInfo.onlineVersion = match[1];
            if (versionInfo.onlineVersion != versionInfo.currentVersion)
                versionInfo.hintClass = 'outdatedVersion';

            JMSLib.HTMLTemplate.applyTemplate(versionInfo, view);
        };
        xhr.send();
    };

    homePage.prototype.onInitialize = function () {
        var me = this;
        var versionAvailable = me.registerAsyncCall();

        // Vorlagen vorbereiten
        me.detailsManager = new JMSLib.DetailManager(1, 'startGuide', 'startScan', 'checkUpdate');

        // Mehr als die Versionsinformationen brauchen wir gar nicht
        VCRServer.getServerVersion().done(function (data) {
            me.serverInfo = data;
            versionAvailable();
        });
    };

    homePage.prototype.onShow = function () {
        var me = this;
        var serverInfo = me.serverInfo;

        // Visualisieren, dass mindestens ein Gerät aktiv ist
        if (serverInfo.active)
            $('.serverActive').removeClass(JMSLib.CSSClass.invisible);

        // Administration deaktivieren, wenn der Anwender kein Administrator ist oder mindestens ein Gerät aktiv ist
        if (serverInfo.active || !serverInfo.userIsAdmin) {
            $('#startAdmin').removeAttr('href');
            $('#startAdminIcon').removeAttr('href');
        }

        // Suchlauf kann nur gestartet werden, wenn dieser nicht über die Konfiguration deaktiviert wurde
        var startScan = $('#startScan');
        if (!serverInfo.canScan)
            startScan.removeAttr('href');
        else
            startScan.click(function () {
                me.showUpdate(startScan, 1, 'sourceScan');
            });

        // Programmzeitschrift kann nur aktualisiert werden, wenn diese nicht über die Konfiguration deaktiviert wurde
        var startGuide = $('#startGuide');
        if (!serverInfo.hasGuides)
            startGuide.removeAttr('href');
        else
            startGuide.click(function () {
                me.showUpdate(startGuide, 0, 'guideUpdate');
            });

        // Die Versionprüfung kann jeder Anwender aufrufen
        var checkUpdate = $('#checkUpdate');
        checkUpdate.click(function () {
            me.checkUpdate(checkUpdate);
        });

        me.title = 'VCR.NET Recording Service ' + serverInfo.version + ' (' + serverInfo.msiVersion + ')';
    };
    homePage.versionExtract = />VCRNET\.MSI<\/a>[^<]*\s([^\s]+)\s*</i;
    return homePage;
})(Page);

// Der Aufzeichnungsplan
var planPage = (function (_super) {
    __extends(planPage, _super);
    function planPage() {
        _super.apply(this, arguments);
        this.title = '';
        this.visibleLinks = '.guideLink, .newLink, .refreshLink, .currentLink';
        // Erster zu berücksichtigender Zeitpunkt
        this.minStart = new Date(0);
        // Letzter zu berücksichtigender Zeitpunkt
        this.maxStart = new Date(0);
    }
    // Aktualisiert die Anzeige
    planPage.prototype.refresh = function () {
        // Filter auf den aktuellen Stand bringen
        this.setFilter();

        // Aktualisierung ausführen
        this.planRowTemplate.refresh();
    };

    // Setzt die Grenzwerte für den Filter neu
    planPage.prototype.setFilter = function () {
        // Die Anzeige wird nun aktualisiert
        this.detailsManager.reset();

        // Auswahl auslesen
        var choice = this.startChooser.find(':checked').val();

        // Auf Mitternacht vorsetzen
        var midnight = new Date(Date.now());
        midnight = new Date(midnight.getFullYear(), midnight.getMonth(), midnight.getDate());

        // Anfangszeitpunkt setzen
        if ((choice === undefined) || choice == '0') {
            window.location.hash = '#plan';

            // Einfach alles
            this.minStart = new Date(0);
            this.maxStart = new Date(midnight.getTime() + 86400000 * VCRServer.UserProfile.global.planDaysToShow);
        } else {
            window.location.hash = '#plan;' + choice;

            // Und Zeitpunkt ermitteln
            this.minStart = new Date(midnight.getTime() + 86400000 * choice);
            this.maxStart = new Date(this.minStart.getTime() + 86400000 * VCRServer.UserProfile.global.planDaysToShow);
        }
    };

    // Führt eine Einschränkung aus
    planPage.prototype.filter = function (item) {
        if (item.start < this.minStart)
            return false;

        if (item.start >= this.maxStart)
            return false;

        if (item.station == 'EPG')
            return this.ckGuide.is(':checked');

        if (item.station == 'PSI')
            return this.ckScan.is(':checked');

        return true;
    };

    // Anzeige der Ausnahmeregelung
    planPage.prototype.editException = function (item, origin) {
        var info = item.exceptionInfo;
        var me = this;

        info.startEdit(item, me.detailsManager.toggle(info, origin, 1), function () {
            me.reload(null);
        });
    };

    // Überlappungsanzeige vorbereiten
    planPage.prepareGuideDisplay = function (entry, template, start, end) {
        // Zeiten ermitteln
        var epgStart = entry.start.getTime();
        var epgEnd = epgStart + entry.duration;
        var recStart = start.getTime();
        var recEnd = end.getTime();
        var fullTime = recEnd - recStart;

        // Zeigen, ob die Sendung vollständig aufgezeichnet wird
        if (recStart <= epgStart)
            if (recEnd >= epgEnd)
                entry.overlapClass = CSSClass.fullRecord;

        // Daten binden
        var html = JMSLib.HTMLTemplate.cloneAndApplyTemplate(entry, template);

        // Anzeige vorbereiten
        if (fullTime > 0)
            if (epgEnd > epgStart) {
                // Anzeigelement ermitteln
                var container = html.find('#guideOverlap');

                // Grenzen korrigieren
                if (epgStart < recStart)
                    epgStart = recStart;
                else if (epgStart > recEnd)
                    epgStart = recEnd;
                if (epgEnd < recStart)
                    epgEnd = recStart;
                else if (epgEnd > recEnd)
                    epgEnd = recEnd;

                // Breiten berechnen
                var left = 90.0 * (epgStart - recStart) / fullTime;
                var middle = 90.0 * (epgEnd - epgStart) / fullTime;
                var right = Math.max(0, 90.0 - middle - left);

                // Elemente suchen
                var all = container.find('div div');
                var preTime = $(all[0]);
                var recTime = $(all[1]);
                var postTime = $(all[2]);

                // Breiten festlegen
                recTime.width(middle + '%');

                // Bei den Rändern kann es auch sein, dass wir die ganz loswerden wollen
                if (right >= 1)
                    postTime.width(right + '%');
                else
                    postTime.remove();
                if (left >= 1)
                    preTime.width(left + '%');
                else
                    preTime.remove();

                // Sichtbar schalten
                container.removeClass(JMSLib.CSSClass.invisible);
            }

        // Fertiges Präsentationselement melden
        return html;
    };

    // Eintrag der Programmzeitschrift abrufen
    planPage.prototype.rowGuideEntry = function (item, origin) {
        var me = this;

        // Verweis deaktivieren
        origin.removeAttribute('href');

        // Prüfen
        if (origin.showingGuide)
            return;

        // Merken
        origin.showingGuide = true;

        // Anzeigeelement suchen
        var guideReferenceElement = $(origin.parentNode.parentNode);
        var guideElement = guideReferenceElement.parent().parent().find('#planRowProgramGuide');

        // Eintrag suchen
        item.requestGuide(function (entry) {
            // Daten binden
            var html = planPage.prepareGuideDisplay(entry, me.guideTemplate, item.start, item.end);

            // Vorbereiten
            html.find('#findInGuide').button();

            // Anzeigen
            guideElement.replaceWith(html);
            guideReferenceElement.addClass(JMSLib.CSSClass.invisible);
        });
    };

    // Aktuellen Aufzeichnungsplan ermitten
    planPage.prototype.reload = function (whenLoaded) {
        var me = this;

        // Wir schauen maximal 13 Wochen in die Zukunft
        var endOfTime = new Date(Date.now() + 13 * 7 * 86400000);

        // Zusätzlich beschränken wir uns auf maximal 500 Einträge
        VCRServer.getPlan(500, endOfTime).done(function (data) {
            // Rohdaten in Modelldaten transformieren
            var plan = $.map(data, function (rawData) {
                var item = new PlanEntry(rawData);

                item.onShowDetails = function (item, origin) {
                    if (me.detailsManager.toggle(item, origin, 0))
                        $(origin.parentNode.parentNode.nextSibling).find('#guideLink').click();
                };
                item.onException = function (item, origin) {
                    me.editException(item, origin);
                };
                item.onShowGuide = function (item, origin) {
                    me.rowGuideEntry(item, origin);
                };

                return item;
            });

            // Details deaktivieren
            me.detailsManager.reset();

            // Liste der Aufzeichnungen aktualisieren
            me.planRowTemplate.loadList(plan);

            // Abschluss melden
            if (whenLoaded != null)
                whenLoaded();
        });
    };

    planPage.prototype.onShow = function () {
        var me = this;

        // Aktuelle Auswahl löschen
        me.startChooser.children().remove();

        // Mitternacht ermitteln
        var midnight = new Date(Date.now());
        midnight = new Date(midnight.getFullYear(), midnight.getMonth(), midnight.getDate());

        // Vorauswahl prüfen
        var hash = window.location.hash;
        var startIndex = hash.indexOf(';');
        var selected = (startIndex >= 0) ? hash.substr(startIndex + 1) : null;
        var rootChecker = (startIndex < 0) ? ' checked="checked"' : '';

        // Neue Auswahl ergänzen
        me.startChooser.append('<input type="radio" id="startChoice0" name="startChoice"' + rootChecker + ' value="0"/><label for="startChoice0">Jetzt</label>');

        for (var i = 1; i < 7; i++) {
            // Werte berechnen
            var days = i * VCRServer.UserProfile.global.planDaysToShow;
            var daysAsString = days.toString();
            var startDate = new Date(midnight.getTime() + 86400000 * days);
            var isSelected = (daysAsString == selected);
            var selector = isSelected ? ' selected="selected"' : '';
            var checker = isSelected ? ' checked="checked"' : '';

            // Neue Auswahl ergänzen
            me.startChooser.append('<input type="radio" id="startChoice' + i + '" name="startChoice"' + checker + ' value="' + daysAsString + '"/><label for="startChoice' + i + '">' + JMSLib.DateFormatter.getShortDate(startDate) + '</label>');
        }

        // Anzeige aufbereiten
        me.startChooser.buttonset();
        me.startChooser.find('input').click(function () {
            me.refresh();
        });

        // Filter aktivieren
        me.planRowTemplate.filter = function (item) {
            return me.filter(item);
        };

        // Und anzeigen
        me.refresh();

        // Aktualisierung aufsetzen
        $('.refreshLink').click(function () {
            me.reload(null);
        });
        me.ckGuide.click(function () {
            me.refresh();
        });
        me.ckScan.click(function () {
            me.refresh();
        });

        me.title = 'Geplante Aufzeichnungen für ' + VCRServer.UserProfile.global.planDaysToShow + ' Tage';
    };

    planPage.prototype.onInitialize = function () {
        var me = this;
        var settingsLoaded = me.registerAsyncCall();
        var guideLoaded = me.registerAsyncCall();
        var planLoaded = me.registerAsyncCall();

        // Elemente suchen
        me.startChooser = $('#startChoice');

        // Darstellung der Auswahl konfigurieren
        me.ckGuide = $('#showGuide');
        me.ckGuide.button();
        me.ckScan = $('#showScan');
        me.ckScan.button();

        // Vorlagen vorbereiten
        me.detailsManager = new JMSLib.DetailManager(2, 'planRowDetails', 'planRowException');
        me.planRowTemplate = JMSLib.HTMLTemplate.dynamicCreate($('#planRows'), 'planRow');

        // Filter deaktivieren
        me.planRowTemplate.filter = function (item) {
            return false;
        };

        // Laden anstossen
        JMSLib.TemplateLoader.load('currentGuide').done(function (template) {
            me.guideTemplate = $(template).find('#innerTemplate');

            guideLoaded();
        });

        // Benutzerprofile überwachen
        VCRServer.UserProfile.global.register(settingsLoaded);

        // Laden anstossen
        me.reload(planLoaded);
    };
    return planPage;
})(Page);

// Die Konfiguration und Administration
var adminPage = (function (_super) {
    __extends(adminPage, _super);
    function adminPage() {
        _super.apply(this, arguments);
        this.title = 'Administration und Konfiguration';
        this.visibleLinks = '.guideLink, .newLink, .planLink, .currentLink';
    }
    // Überträgt die Liste der Verzeichnisse in die Oberfläche
    adminPage.prototype.fillDirectories = function (directories) {
        var selDir = this.directoryBrowser;

        selDir.children().remove();

        $.each(directories, function (index, directory) {
            if (directory == null)
                selDir.append(new Option('<Bitte auswählen>', ''));
            else
                selDir.append(new Option(directory));
        });
    };

    adminPage.prototype.onInitialize = function () {
        var me = this;
        var loadFinished = me.registerAsyncCall();

        me.directoryBrowser = $('#browseDirectory');

        // Gruppen nur einmal laden
        if (adminPage.groups == null) {
            var groupsLoaded = me.registerAsyncCall();

            VCRServer.getWindowsGroups().done(function (data) {
                adminPage.groups = data;
                groupsLoaded();
            });
        }

        // Alles nacheinander laden - die Zahl der gleichzeitig offenen Requests ist beschränkt!
        VCRServer.browseDirectories('', true).then(function (directories) {
            me.fillDirectories(directories);
            return VCRServer.getSecuritySettings();
        }).then(function (data) {
            me.security = data;
            return VCRServer.getDirectorySettings();
        }).then(function (data) {
            me.directory = data;
            return VCRServer.getGuideSettings();
        }).then(function (data) {
            me.guide = data;
            return VCRServer.getSourceScanSettings();
        }).then(function (data) {
            me.scan = data;
            return VCRServer.getProfileSettings();
        }).then(function (data) {
            me.devices = data;
            return JMSLib.TemplateLoader.load('adminDevices');
        }).then(function (template) {
            $('#devices').append($(template).find('#template').children());

            return VCRServer.getOtherSettings();
        }).then(function (data) {
            me.other = data;
            return VCRServer.getSchedulerRules();
        }).then(function (data) {
            me.rules = data;
            return JMSLib.TemplateLoader.load('adminRules');
        }).then(function (template) {
            $('#rules').append($(template).find('#template').children());

            loadFinished();
        });

        // Geräte laden
        var profiles = $('#profileForGuide');
        var sourcesLoaded = me.registerAsyncCall();

        me.loadProfiles(profiles);
        me.sourceSelections = new SourceSelectorLoader(profiles);
        me.sourceSelections.loadTemplates(function () {
            me.sources = me.sourceSelections.appendAfter($('#guideSourceSelector').find('tbody').children().last(), true);

            sourcesLoaded();
        });

        // Vorlagen laden
        var templateLoaded = me.registerAsyncCall();
        JMSLib.TemplateLoader.load('profileRow').done(function (template) {
            me.deviceTemplate = $(template).find('#template');

            templateLoaded();
        });

        // Uhrzeiten laden
        JMSLib.HourListSettings.createHourButtons($('.guideHours'), 'guideHour');
        JMSLib.HourListSettings.createHourButtons($('.scanHours'), 'scanHour');

        // Und schließlich brauchen wir noch die Konfiguration des Anwenders
        VCRServer.UserProfile.global.register(me.registerAsyncCall());
    };

    // Schaltet alle Oberflächen Elemnte ab und zeigt vor dem verzögerten Übergang auf die Startseite eine knappe Information an.
    adminPage.restartServer = function () {
        $('.adminView').addClass(JMSLib.CSSClass.invisible);
        $('.linkArea').addClass(JMSLib.CSSClass.invisible);
        $('.serverRestart').removeClass(JMSLib.CSSClass.invisible);

        window.setTimeout(function () {
            window.location.reload();
        }, 10000);
    };

    // Aktualisiert Konfigurationsdaten und kehrt dann im Allgemeinen auf die Startseite zurück.
    adminPage.prototype.update = function (type, contract, button, filter) {
        if (typeof filter === "undefined") { filter = null; }
        button.removeAttr('title');

        VCRServer.updateConfiguration(type, contract, filter).done(function (data) {
            // Ergebnis bearbeiten
            if (data == null)
                button.addClass(CSSClass.danger);
            else if (data)
                adminPage.restartServer();
            else {
                VCRServer.RecordingDirectoryCache.reset();

                window.location.hash = 'home';
            }
        }).fail(JMSLib.dispatchErrorMessage(function (message) {
            // Fehler bearbeiten
            button.attr('title', message);
            button.addClass(CSSClass.danger);
        }));
    };

    // Ergänzt ein Verzeichnis.
    adminPage.prototype.addDirectory = function () {
        var shareInput = $('#networkShare');
        var list = $('#recordingDirectories');

        var share = shareInput.val();
        if (share != null)
            if (share.length > 0) {
                VCRServer.validateDirectory(share).done(function (ok) {
                    JMSLib.Bindings.setErrorIndicator(shareInput, ok ? null : 'Ungültiges Verzeichnis.');

                    if (!ok)
                        return;

                    shareInput.val(null);
                    list.append(new Option(share));
                });

                return;
            }

        var selected = $('#browseDirectory').val();
        if (selected == null)
            return;
        if (selected.length < 1)
            return;

        list.append(new Option(selected));
    };

    // Bereitet die Anzeige der Sicherheitseinstellungen vor
    adminPage.prototype.showSecurity = function () {
        var me = this;

        // Auswahllisten für Benutzer
        var selUser = $('#selUserGroup');
        var selAdmin = $('#selAdminGroup');

        // Benutzergruppen laden
        $.each(adminPage.groups, function (index, group) {
            selUser.append(new Option(group));
            selAdmin.append(new Option(group));
        });

        // Speichern vorbereiten
        var securityUpdate = $('#updateSecurity');
        securityUpdate.click(function () {
            me.update('security', me.security, securityUpdate);
        });
    };

    // Bereitet die Anzeige der Verzeichnisse vor
    adminPage.prototype.showDirectory = function () {
        var me = this;

        // Oberflächenelemente
        var directoryUpdate = $('#updateDirectory');
        var recording = $('#recordingDirectories');
        var discard = $('#removeDirectory');
        var toParent = $('#toParentDir');
        var accept = $('#acceptDir');

        // Aktuelle Auswahl
        $.each(me.directory.directories, function (index, directory) {
            recording.append(new Option(directory));
        });

        discard.click(function () {
            recording.find(':checked').remove();
        });
        accept.click(function () {
            me.addDirectory();
        });

        // Navigation
        toParent.click(function () {
            var root = me.directoryBrowser.children().first().val();

            VCRServer.browseDirectories(root, false).done(function (directories) {
                me.fillDirectories(directories);
            });
        });
        me.directoryBrowser.change(function () {
            var dir = me.directoryBrowser.val();
            if (dir == null)
                return;
            if (dir.length < 1)
                return;

            VCRServer.browseDirectories(dir, true).done(function (directories) {
                me.fillDirectories(directories);
            });
        });

        // Speichern
        directoryUpdate.click(function () {
            me.directory.directories = $.map($('#recordingDirectories option'), function (option) {
                return option.value;
            });

            me.update('directory', me.directory, directoryUpdate);
        });
    };

    // Bereitet die Konfiguration der Programmzeitschrift vor
    adminPage.prototype.showGuide = function () {
        var me = this;

        // Auswahl neuer Quellen vorbereiten
        me.sources.initialize();

        // Oberflächenelemente
        var guideUpdate = $('#updateGuide');
        var discard = $('#removeFromGuide');
        var selSource = $('#guideSources');
        var selector = selSource[0];
        var accept = $('#addToGuide');

        // Aktuelle Auswahl
        $.each(me.guide.sources, function (index, source) {
            selSource.append(new Option(source));
        });

        discard.click(function () {
            selSource.find(':checked').remove();
        });
        accept.click(function () {
            var source = me.guide['sourceName'];
            if (source == null)
                return;
            if (source.length < 1)
                return;

            var options = selector.options;
            var count = options.length;
            for (var i = 0; i < count; i++)
                if (options[i].value == source)
                    return;

            selSource.append(new Option(source));

            selector.selectedIndex = count;

            me.sources.reset();
        });

        // Speichern
        guideUpdate.click(function () {
            if (GuideSettingsValidator.activated(me.guide))
                me.guide.sources = $.map($('#guideSources option'), function (option) {
                    return option.value;
                });
            else
                me.guide.duration = 0;

            me.update('guide', me.guide, guideUpdate, GuideSettingsValidator.filterPropertiesOnSend);
        });
    };

    // Bereitet die Konfiguration für die Aktualisierung der Quellen vor
    adminPage.prototype.showScan = function () {
        var me = this;

        // Oberflächenelemente
        var scanUpdate = $('#updateScan');

        // Speichern
        scanUpdate.click(function () {
            switch (SourceScanSettingsValidator.mode(me.scan)) {
                case 'D':
                    me.scan.interval = 0;
                    break;
                case 'M':
                    me.scan.interval = -1;
                    break;
                case 'P':
                    break;
            }

            me.update('scan', me.scan, scanUpdate, SourceScanSettingsValidator.filterPropertiesOnSend);
        });
    };

    // Bereitet die Konfiguration für die Nutzung der Geräteprofile vor
    adminPage.prototype.showDevices = function () {
        var me = this;

        // Oberflächenelemente
        var deviceUpdate = $('#updateDevices');
        var devices = $('#selDefaultProfile');

        $.each(me.devices.profiles, function (index, profile) {
            devices.append(new Option(profile.name));
        });

        // Speichern
        deviceUpdate.click(function () {
            me.update('devices', me.devices, deviceUpdate);
        });
    };

    // Bereitet die Konfiguration sonstiger Einstellungen vor
    adminPage.prototype.showOther = function () {
        var me = this;

        // Oberflächenelemente
        var otherUpdate = $('#updateOther');

        // Speichern
        otherUpdate.click(function () {
            var settings = me.other;

            switch (OtherSettingsValidator.hibernationMode(settings)) {
                case 'OFF':
                    settings.mayHibernate = false;
                    break;
                case 'S3':
                    settings.mayHibernate = true;
                    settings.useStandBy = true;
                    break;
                case 'S4':
                    settings.mayHibernate = true;
                    settings.useStandBy = false;
                    break;
            }

            me.update('other', settings, otherUpdate);
        });
    };

    // Bereitet die Eingabe der Planungsregeln vor
    adminPage.prototype.showRules = function () {
        var me = this;

        // Oberflächenelemente
        var rulesUpdate = $('#updateRules');

        // Speichern
        rulesUpdate.click(function () {
            me.update('rules', me.rules, rulesUpdate);
        });
    };

    adminPage.prototype.onShow = function () {
        var me = this;

        var navigator = $('#adminTabs');
        var options = {};

        // Eventuell sollen wir in einer bestimmten Situation starten
        var hash = window.location.hash;
        var currentTabIndex = hash.indexOf(';');
        if (currentTabIndex >= 0)
            switch (hash.substr(currentTabIndex + 1)) {
                case 'directories':
                    options.active = 1;
                    break;
                case 'security':
                    options.active = 0;
                    break;
                case 'devices':
                    options.active = 2;
                    break;
                case 'sources':
                    options.active = 4;
                    break;
                case 'guide':
                    options.active = 3;
                    break;
                case 'rules':
                    options.active = 5;
                    break;
                case 'other':
                    options.active = 6;
                    break;
            }

        // Oberfläche vorbereiten
        navigator.tabs(options).addClass('ui-tabs-vertical ui-helper-clearfix');
        navigator.on('tabsactivate', function (ev) {
            window.location.hash = 'admin;' + arguments[1].newPanel.selector.substr(1);
        });

        // Oberfläche vorbereiten
        $('.editButtons').button();

        // Alle Bereiche vorbereiten
        me.showDirectory();
        me.showSecurity();
        me.showDevices();
        me.showOther();
        me.showGuide();
        me.showRules();
        me.showScan();

        // Besondere Teilaspekte
        var profileValidator = new ProfileSettingsValidator(me.devices, $('#updateDevices'));

        // An die Oberfläche binden
        JMSLib.Bindings.bind(new DirectorySettingsValidator(me.directory, $('#updateDirectory')), $('#directories'));
        JMSLib.Bindings.bind(new GuideSettingsValidator(me.guide, $('#updateGuide'), $('#guideForm')), $('#guide'));
        JMSLib.Bindings.bind(new SourceScanSettingsValidator(me.scan, $('#updateScan')), $('#sources'));
        JMSLib.Bindings.bind(new OtherSettingsValidator(me.other, $('#updateOther')), $('#other'));
        JMSLib.Bindings.bind(new SecuritySettingsValidator(me.security), $('#security'));
        JMSLib.Bindings.bind(new ScheduleRulesValidator(me.rules), $('#rules'));
        JMSLib.Bindings.bind(profileValidator, $('#devices'));

        // Besondere Teilaspekte
        profileValidator.bindProfiles($('#deviceList'), me.deviceTemplate);

        // Oberfläche vorbereiten
        $('.' + JMSLib.CSSClass.hourSetting).button();
    };
    adminPage.groups = null;
    return adminPage;
})(Page);

// Alle Aktivitäten
var currentPage = (function (_super) {
    __extends(currentPage, _super);
    function currentPage() {
        _super.apply(this, arguments);
        this.title = 'Geräteübersicht';
        this.visibleLinks = '.refreshLink, .guideLink, .planLink, .newLink';
    }
    // Führt eine vollständige Aktualisierung aus
    currentPage.prototype.reload = function () {
        var me = this;

        CurrentInfo.load(function (infos) {
            me.detailsManager.reset();
            me.table.loadList(infos);
        });
    };

    // Anzeige der Programmzeitschrift einer einzelnen Aktivität
    currentPage.prototype.showGuide = function (item, origin) {
        var detailsManager = this.detailsManager;

        item.requestGuide(function (entry) {
            // Anzeige vorbereiten
            var view = detailsManager.toggle(entry, origin, 0, function (guideItem, template) {
                return planPage.prepareGuideDisplay(guideItem, template, item.start, item.end);
            });

            if (view != null)
                view.find('#findInGuide').button();
        });
    };

    // Eine laufende Aufzeichnung manipulieren
    currentPage.prototype.startAbort = function (item, origin) {
        var me = this;

        // Auswahlwerte zurücksetzen
        item.suppressHibernate = VCRServer.UserProfile.global.noHibernateOnAbort;
        item.remainingMinutes = item.originalRemainingMinutes;

        var template = me.detailsManager.toggle(item, origin, 1);
        if (template == null)
            return;

        // Aktualisierung
        function refresh() {
            JMSLib.HTMLTemplate.applyTemplate(item, template);
        }

        // Konfiguration der Einstellungen
        var options = {
            slide: function (slider, newValue) {
                item.remainingMinutes = newValue.value;
                refresh();
            },
            value: item.remainingMinutes,
            max: 480,
            min: 0
        };

        var slider = template.find('#slider');
        var sendButton = template.find('#sendButton');
        var disableButton = template.find('#disableButton');

        // Oberfläche aufbereiten
        slider.slider(options);
        disableButton.button();
        sendButton.button();

        // Aktionen definiert
        sendButton.click(function () {
            sendButton.button('disable', true);
            item.updateEndTime(function () {
                me.reload();
            });
        });
        disableButton.click(function () {
            slider.slider('value', item.remainingMinutes = 0);
            refresh();
        });
    };

    currentPage.prototype.onShow = function () {
        var me = this;

        $('.refreshLink').click(function () {
            me.reload();
        });
    };

    currentPage.prototype.onInitialize = function () {
        var me = this;
        var profileAvailable = me.registerAsyncCall();
        var tableAvailable = me.registerAsyncCall();

        // Vorlagen laden
        me.detailsManager = new JMSLib.DetailManager(2, 'currentGuide', 'editCurrent');
        me.table = JMSLib.HTMLTemplate.dynamicCreate($('#currentTable'), 'currentRow');

        // Ereignisse anmelden
        CurrentInfo.guideDisplay = function showGuide(item, origin) {
            me.showGuide(item, origin);
        };
        CurrentInfo.startAbort = function startAbort(item, origin) {
            me.startAbort(item, origin);
        };

        // Wir brauchen auch die Benutzereinstellungen
        VCRServer.UserProfile.global.register(profileAvailable);

        // Und natürlich die eigentlichen Daten
        CurrentInfo.load(function (infos) {
            me.table.loadList(infos);
            tableAvailable();
        });
    };
    return currentPage;
})(Page);

// Aufzeichung anlegen, bearbeiten und löschen
var editPage = (function (_super) {
    __extends(editPage, _super);
    function editPage() {
        _super.apply(this, arguments);
        this.title = '';
        this.visibleLinks = '.guideLink, .planLink, .currentLink';
    }
    editPage.prototype.onShow = function () {
        var me = this;

        // Modelldaten erstellen
        var job = new JobDataValidator(me.existingData, $('#update'), me.sourceSelections.profileSelector.val());
        var schedule = new ScheduleDataValidator(job, me.existingData);

        // Rücksprung vorbereiten.
        if (me.fromGuide)
            job.model.guideAfterAdd = VCRServer.UserProfile.global.guideAfterAdd;

        // Datenübertragung vorbereiten
        JMSLib.Bindings.bind(job, $('#jobData'));
        JMSLib.Bindings.bind(schedule, $('#scheduleData'));

        // Quellenauswahl vorbereiten
        me.jobSources.initialize();
        me.scheduleSources.initialize();

        // Überschrift setzen
        if (job.model.id == null)
            me.title = 'Neuen Auftrag anlegen';
        else if (schedule.model.id == null)
            me.title = 'Neue Aufzeichnung anlegen';
        else {
            me.title = 'Aufzeichnung bearbeiten';

            var deleteButton = $('#delete');
            deleteButton.button();
            deleteButton.click(function (scope) {
                job.prepareDelete(scope);
            });
            deleteButton.removeClass(JMSLib.CSSClass.invisible);
        }

        // Ausnahmen übernehmen
        var exceptions = schedule.model.exceptionInfos;
        if (exceptions.length > 0)
            me.exceptionRowTemplate.loadList(exceptions);
        else
            $('#exceptionArea').addClass(JMSLib.CSSClass.invisible);
    };

    editPage.prototype.onInitialize = function () {
        var me = this;
        var templatesLoaded = me.registerAsyncCall();
        var directoriesLoaded = me.registerAsyncCall();

        // Die Informationen, die wir gerade verändern
        me.sourceSelections = new SourceSelectorLoader($('#selProfile'));
        me.exceptionRowTemplate = JMSLib.HTMLTemplate.dynamicCreate($('#exceptionRows'), 'exceptionRow');

        // Auslesen der Kennung
        var query = window.location.hash;
        var idIndex = query.indexOf(';id=');
        var hasId = (idIndex >= 0);
        var jobScheduleId = hasId ? query.substr(idIndex + 4) : '';
        var epgIndex = jobScheduleId.indexOf(';epgid=');
        var epgId = '';

        if (me.fromGuide = (epgIndex >= 0)) {
            epgId = '?epg=' + jobScheduleId.substr(epgIndex + 7);
            jobScheduleId = jobScheduleId.substr(0, epgIndex);
        }

        // Konfiguration der Datumsauswahl
        var germanMonths = ['Jan', 'Feb', 'Mrz', 'Apr', 'Mai', 'Jun', 'Jul', 'Aug', 'Sep', 'Okt', 'Nov', 'Dez'];
        var startDate = $('#startDate');
        var endDate = $('#endDate');
        var dateOptions = {
            dayNamesMin: JMSLib.DateFormatter.germanDays,
            maxDate: ScheduleData.maximumDate,
            monthNamesShort: germanMonths,
            selectOtherMonths: true,
            closeText: 'Schließen',
            dateFormat: 'dd.mm.yy',
            showButtonPanel: true,
            showOtherMonths: true,
            currentText: 'Heute',
            changeMonth: true,
            changeYear: true,
            firstDay: 1
        };

        // Datumsauswahl vorbereiten
        startDate.datepicker(dateOptions);
        endDate.datepicker(dateOptions);

        // Daten zur Aufzeichnung laden
        if (hasId) {
            var epgLoaded = me.registerAsyncCall();

            VCRServer.createScheduleFromGuide(jobScheduleId, epgId).done(function (data) {
                me.existingData = data;
                epgLoaded();
            });
        }

        // Liste der Geräteprofile laden
        me.loadProfiles($('#selProfile'));

        // Liste der Verzeichnisse wählen
        VCRServer.RecordingDirectoryCache.load().done(function (directories) {
            var directoryList = $('#selDirectory');

            $.each(directories, function (index, directory) {
                directoryList.append(new Option(directory));
            });

            directoriesLoaded();
        });

        // Vorlage für die Auswahl der Quellen laden
        me.sourceSelections.loadTemplates(function () {
            // In die Oberfläche einbinden
            me.jobSources = me.sourceSelections.appendAfter($('#jobData').find('tbody').children().last());
            me.scheduleSources = me.sourceSelections.appendAfter($('#scheduleData').find('tbody').children().first());

            templatesLoaded();
        });

        // Besondere Aktionen sobald die Benutzerkonfiguration verändert wurde
        VCRServer.UserProfile.global.register(me.registerAsyncCall());
    };
    return editPage;
})(Page);

// Die Programmzeitschrift
var guidePage = (function (_super) {
    __extends(guidePage, _super);
    function guidePage() {
        _super.apply(this, arguments);
        this.title = 'Programmzeitschrift';
        this.visibleLinks = '.newLink, .planLink, .currentLink';
    }
    // Klappt die Detailansicht auf oder zu
    guidePage.prototype.showDetails = function (guideItem, origin) {
        var me = this;

        var view = me.details.toggle(guideItem, origin, 0);
        if (view == null)
            return;

        // Referenzelement in der Anzeige ermitteln
        var guideReferenceElement = $(origin.parentNode.parentNode);
        var guideElement = view.find('#planRowProgramGuide');

        // Anzeigen
        var details = JMSLib.HTMLTemplate.cloneAndApplyTemplate(guideItem, me.guideTemplate);
        details.find('#guideHeadline').addClass(JMSLib.CSSClass.invisible);
        details.find('#findInGuide').button();

        // Auswahlliste mit den Aufträgen füllen
        var jobSelector = view.find('#selJob');
        $.each(me.jobs, function (index, job) {
            jobSelector.append(new Option(job.name, '*' + job.id));
        });

        // Fertig aufbereitetes Element einsetzen
        guideElement.replaceWith(details);

        // Schaltfläche zum Anlegen einer neuen Aufzeichnung aktivieren
        var createCommand = view.find('#createNew');
        createCommand.button();

        // Bei jeder Änderung der Auswahl des Auftrags wird das Ziel geeignet angepasst und so das normale Verhalten eines Verweises erreicht
        function jobChanged() {
            createCommand.attr('href', '#edit;id=' + jobSelector.val() + ';epgid=' + guideItem.id);
        }

        // Wird ein Auftrag ausgewählt muss der Verweis gesetzt werden
        jobSelector.change(jobChanged);

        // Auf jeden Fall aber einmalig vor der Anzeige
        jobChanged();
    };

    // Aktualisiert die Liste der Sendungen
    guidePage.prototype.refresh = function () {
        var me = this;

        $('#addFavorite').button('option', 'disabled', GuideFilter.global.title == null);

        GuideFilter.global.execute(function (items) {
            $.each(items, function (index, item) {
                item.onShowDetails = function (target, origin) {
                    me.showDetails(target, origin);
                };
            });

            me.details.reset();
            me.guideTable.loadList(items);

            // Schauen wir mal, ob wir blättern können
            $('.firstButton').button('option', 'disabled', GuideFilter.global.index < 1);
            $('.prevButton').button('option', 'disabled', GuideFilter.global.index < 1);
            $('.nextButton').button('option', 'disabled', !GuideFilter.global.canPageForward());

            // Die Liste als Ganzes ausblenden, wenn sie leer ist
            var display = $('.displayArea');
            if (items.length > 0)
                display.removeClass(JMSLib.CSSClass.invisible);
            else
                display.addClass(JMSLib.CSSClass.invisible);
        });
    };

    // Wird aufgerufen, wenn der Anwender das Gerät verändert
    guidePage.prototype.deviceChanged = function (filterIsCurrent) {
        var me = this;
        var newDevice = $('#selDevice').val();

        // Liste leeren
        me.jobs = new Array();

        // Aufträge zum Gerät anfordern
        VCRServer.getProfileJobInfos(newDevice).done(function (jobs) {
            me.jobs = jobs;

            // Infomationen zum Gerät anfordern
            VCRServer.GuideInfoCache.getInfo(newDevice).done(function (data) {
                // Die aktuelle Quelle im Filter
                var station = GuideFilter.global.station;
                var info = new VCRServer.GuideInfo(data);

                // Liste der Quellen ganz neu füllen
                var selSource = $('#selSource');
                selSource.children().remove();
                selSource.append(new Option('(Alle Sender)', ''));
                $.each(info.stations, function (index, station) {
                    selSource.append(new Option(station));
                });

                // Dann versuchen wir, den ursprünglich gewählten Wert zu reaktivieren
                selSource.val(station);

                // Liste der Tage ganz neu füllen
                var selDay = $('#selectDay');
                selDay.children().remove();
                selDay.append('<a href="javascript:void(0)" data-day="0">Jetzt</a>');
                var firstDay = info.firstStart;
                if (firstDay != null) {
                    firstDay = new Date(firstDay.getFullYear(), firstDay.getMonth(), firstDay.getDate());

                    for (var n = 14; (n-- > 0) && (firstDay < info.lastStart);) {
                        selDay.append('<a href="javascript:void(0)" data-day="' + firstDay.toISOString() + '">' + JMSLib.DateFormatter.getShortDate(firstDay) + '</a>');

                        firstDay = new Date(firstDay.getTime() + 86400000);
                    }
                }

                // Änderungen an der Auswahl der Tage ab nun überwachen
                var daySelectors = selDay.find('a');
                daySelectors.button();
                daySelectors.click(function (ev) {
                    me.dayChanged(ev);
                });

                // Nun können wir eine Aktualisierung anfordern
                if (!filterIsCurrent)
                    GuideFilter.global.changeDevice(newDevice, selSource.val());
            });
        });
    };

    // Der Anwender hat eine Uhrzeit ausgewählt, ab der die Anzeige erfolgen soll
    guidePage.prototype.hourChanged = function (eventObject) {
        // Was hat der Anwender gewählt?
        var target = eventObject.currentTarget;
        var hour = parseInt(target.getAttribute('data-hour')) * 3600000;

        // Den aktuellen Tag ermitteln
        var start = GuideFilter.global.start;
        if (start == null)
            start = new Date($.now()).toISOString();

        // Tag und Uhrzeit kombinieren und aktualisieren
        var fullStart = new Date(start);
        var dayStart = new Date(fullStart.getFullYear(), fullStart.getMonth(), fullStart.getDate());

        GuideFilter.global.changeStart(new Date(dayStart.getTime() + hour).toISOString());
    };

    // Der Anwender hat einen Tag ausgewählt, ab dem die Anzeige erfolgen soll
    guidePage.prototype.dayChanged = function (eventObject) {
        // Was hat der Anwender gewählt?
        var target = eventObject.currentTarget;

        // Tag ermitteln und anzeigen
        var newStart = target.getAttribute('data-day');
        if (newStart === '0')
            newStart = null;

        GuideFilter.global.changeStart(newStart);
    };

    // Aktiviert oder deaktiviert die Sicht auf die gespeicherten Suchen
    guidePage.prototype.toggleFavorites = function (origin) {
        var me = this;

        SavedGuideQuery.onDeleted = null;

        var view = me.favorites.toggle({}, origin, 0);
        if (view == null)
            return;

        // Liste mit den aktuellen Favoriten aufbauen
        var table = JMSLib.HTMLTemplate.dynamicCreate(view.find('#favoriteList'), 'favoriteRow');
        var queries = SavedGuideQueries.load();

        table.loadList(queries);

        // Sobald die Zähler zur Verfügung stehen zweigen wir nur neu an
        SavedGuideQuery.onCountLoaded = function (query) {
            table.loadList(queries);
        };

        // Beim Löschen muss die Änderung gespeichert und die Anzeige aktualisiert werden
        SavedGuideQuery.onDeleted = function (query) {
            for (var i = 0; i < queries.length; i++)
                if (queries[i] === query) {
                    queries.splice(i, 1);

                    SavedGuideQueries.save();

                    table.loadList(queries);

                    break;
                }
        };

        // Zum Hinzufügen
        var add = $('#addFavorite');
        add.button({ disabled: GuideFilter.global.title == null });
        add.click(function () {
            if (GuideFilter.global.title == null)
                return;

            var query = new SavedGuideQuery();
            query.titleOnly = GuideFilter.global.content == null;
            query.source = GuideFilter.global.station;
            query.device = GuideFilter.global.device;
            query.text = GuideFilter.global.title;

            queries.splice(0, 0, query);

            SavedGuideQueries.save();

            table.loadList(queries);
        });

        // Auswahl übernehmen
        SavedGuideQuery.onClick = function (query) {
            $('#withContent').prop('checked', !query.titleOnly);
            $('#searchText').val(query.text.substr(1));
            $('#withContent').button('refresh');
            $('#selDevice').val(query.device);
            $('#selSource').val(query.source);

            GuideFilter.global.content = query.titleOnly ? null : query.text;
            GuideFilter.global.station = query.source;
            GuideFilter.global.device = query.device;
            GuideFilter.global.title = query.text;

            GuideFilter.global.changeStart(null);

            me.deviceChanged(true);
        };

        // Wir bieten auch hier eine eingebettete Hilfe an
        JMSLib.activateHelp();
    };

    guidePage.prototype.onShow = function () {
        $('.firstButton, .prevButton, .nextButton, .guideHours a, .scanHours a, #resetAll, #favorites').button();

        // Wenn wir Geräteprofile haben, müssen wir etwas mehr tun
        if (this.profiles.length > 0) {
            // Die Auswahlliste der Geräte füllen
            var selDevice = $('#selDevice');
            $.each(this.profiles, function (index, profile) {
                selDevice.append(new Option(profile.name));
            });

            // Vorauswahl aus dem Filter übernehmen
            var device = GuideFilter.global.device;
            if (device != null)
                selDevice.val(device);

            // Von nun an auf Änderungen überwachen
            var me = this;
            selDevice.change(function () {
                me.deviceChanged(false);
            });

            // Und eventuell verändert zurück übertragen
            GuideFilter.global.changeDevice(selDevice.val());

            // Suchtext vorbereiten
            var searchText = $('#searchText');
            var checkContent = $('#withContent');

            // Vorauswahl aus dem Filter übernehmen
            var query = GuideFilter.global.title;
            if (query != null) {
                checkContent.prop('checked', GuideFilter.global.content != null);
                searchText.val(query.substr(1));
            }

            checkContent.button();

            // Von nun an auf Änderungen überwachen
            function textChanged() {
                GuideFilter.global.changeQuery(searchText.val(), checkContent.prop('checked'));
            }
            searchText.on('change', textChanged);
            searchText.on('input', textChanged);
            checkContent.change(textChanged);

            // Sonstige Änderungen überwachen
            var selSource = $('#selSource');
            selSource.change(function (ev) {
                GuideFilter.global.changeStation(selSource.val());
            });
            $('.guideHours a').click(function (ev) {
                me.hourChanged(ev);
            });
            $('.firstButton').click(function () {
                GuideFilter.global.firstPage();
            });
            $('.prevButton').click(function () {
                GuideFilter.global.prevPage();
            });
            $('.nextButton').click(function () {
                GuideFilter.global.nextPage();
            });
            $('#favorites').click(function () {
                me.toggleFavorites(this);
            });
            $('#resetAll').click(function () {
                $('#withContent').prop('checked', true);
                $('#withContent').button('refresh');
                searchText.val(null);
                selSource.val(null);

                GuideFilter.global.reset();
            });

            // Von nun an auf den Filter reagieren
            GuideFilter.global.onChange = function () {
                me.refresh();
            };

            // Erste Aktualisierung
            me.deviceChanged(false);
        }
    };

    guidePage.prototype.onInitialize = function () {
        var me = this;
        var profilesLoaded = me.registerAsyncCall();
        var settingsLoaded = me.registerAsyncCall();
        var guideLoaded = me.registerAsyncCall();

        // Vorlagen verbinden
        me.guideTable = JMSLib.HTMLTemplate.dynamicCreate($('#guideTable'), 'guideRow');
        me.details = new JMSLib.DetailManager(2, 'guideDetails');
        me.favorites = new JMSLib.DetailManager(2, 'favorites');

        // Liste der Geräteprofile laden
        VCRServer.ProfileCache.load().done(function (data) {
            me.profiles = data;

            profilesLoaded();
        });

        // Zusätzliche Vorlagen laden
        JMSLib.TemplateLoader.load('currentGuide').done(function (template) {
            me.guideTemplate = $(template).find('#innerTemplate');

            guideLoaded();
        });

        // Benutzereinstellungen abwarten
        VCRServer.UserProfile.global.register(function () {
            GuideFilter.global.userProfileChanged();
            settingsLoaded();
        });
    };
    return guidePage;
})(Page);

// Die Liste aller Aufträge
var jobsPage = (function (_super) {
    __extends(jobsPage, _super);
    function jobsPage() {
        _super.apply(this, arguments);
        this.title = 'Alle Aufträge';
        this.visibleLinks = '.guideLink, .newLink, .planLink, .currentLink';
    }
    jobsPage.prototype.onInitialize = function () {
        var me = this;
        var loaded = me.registerAsyncCall();

        // Aus der Vorlage erzeugen
        me.isActive = $('#selActive');
        me.table = JMSLib.HTMLTemplate.dynamicCreate($('#jobTable'), 'jobRow');
        me.table.filter = function (row) {
            return row.isActive == me.isActive.is(':checked');
        };

        // Voreingestellte Auswahl setzen - das müssen wir machen, bevor wir die Darstellung umstellen
        var showArchive = (window.location.hash.indexOf(';archive') >= 0);
        if (showArchive)
            $('#selArchive').prop('checked', true);
        else
            me.isActive.prop('checked', true);

        // Auswahl aufbereiten
        var filter = $('#filter');
        filter.buttonset();
        filter.change(function () {
            me.table.refresh();
        });

        // Ladevorgang ausführen
        InfoJob.load(function (rows) {
            me.table.loadList(rows);
            loaded();
        });
    };
    return jobsPage;
})(Page);

// Die Liste der Protokolle
var logPage = (function (_super) {
    __extends(logPage, _super);
    function logPage() {
        _super.apply(this, arguments);
        this.title = 'Aufzeichnungsprotokolle einsehen';
        this.visibleLinks = '.guideLink, .newLink, .planLink, .currentLink';
    }
    // Prüft einen Eintrag
    logPage.prototype.filter = function (entry) {
        if (entry.source == 'EPG')
            return $('#withGuide').is(':checked');
        if (entry.source == 'PSI')
            return $('#withScan').is(':checked');
        if (entry.source == 'LIVE')
            return $('#withLive').is(':checked');

        return true;
    };

    // Aktualisiert lediglich die Filter auf der aktuellen Menge von Protokolleinträgen
    logPage.prototype.refresh = function () {
        this.detailsManager.reset();
        this.rowTemplate.refresh();
    };

    // Fordert neue Protokolleinträge an
    logPage.prototype.reload = function (whenLoaded) {
        if (typeof whenLoaded === "undefined") { whenLoaded = null; }
        var me = this;
        var profile = $('#selProfile').val();
        var endDay = new Date($('#selDate').val());
        var startDay = new Date(endDay.getTime() - 7 * 86400000);

        VCRServer.getProtocolEntries(profile, startDay, endDay).done(function (entries) {
            // Wir zeigen den neuesten oben
            entries.reverse();

            var models = $.map(entries, function (entry) {
                var model = new ProtocolEntry(entry);

                // Detailanzeige vorbereiten
                model.showDetails = function () {
                    // Das war ein Ausblenden der Detailanzeige
                    var view = me.detailsManager.toggle(model, this, 0);
                    if (view == null)
                        return;

                    // Mal schauen, ob es noch Aufzeichnungsdateien gibt
                    var fileUI = view.find('#files');
                    var files = model.files;

                    // Aufzeichnungsdatein zur Anzeige anbieten
                    if (files.length < 1)
                        fileUI.parent().remove();
                    else
                        $.each(files, function (index, url) {
                            fileUI.append('<a href="' + url + '"><img src="ui/images/recording.png" class="linkImage"/></img>');
                        });

                    // Es gibt keine Primärdatei, deren Namen wir anzeigen könnten
                    if ((model.primaryFile == null) || (model.primaryFile.length < 1))
                        $('#primaryFile').parent().remove();
                };

                return model;
            });

            // Neu laden
            me.detailsManager.reset();
            me.rowTemplate.loadList(models);

            // Fertig - beim Starten wartet jemand auf uns
            if (whenLoaded != null)
                whenLoaded();
        });
    };

    logPage.prototype.onInitialize = function () {
        var me = this;
        var profilesLoaded = me.registerAsyncCall();
        var rawNow = new Date($.now());
        var dayList = $('#selDate');

        // Referenz setzen
        me.referenceDay = new Date(Date.UTC(rawNow.getFullYear(), rawNow.getMonth(), rawNow.getDate()));

        for (var i = 0; i < 10; i++) {
            var curDay = new Date(me.referenceDay.getTime() - 7 * i * 86400000);
            var curMonth = 1 + curDay.getUTCMonth();
            var curDate = curDay.getUTCDate();

            dayList.append(new Option(JMSLib.DateFormatter.formatNumber(curDate) + '.' + JMSLib.DateFormatter.formatNumber(curMonth), curDay.toISOString()));
        }

        // Auf Änderungen reagieren
        dayList.change(function () {
            me.reload();
        });

        // Vorlagen vorbereiten
        me.rowTemplate = JMSLib.HTMLTemplate.dynamicCreate($('#logRows'), 'logRow');
        me.rowTemplate.filter = function (item) {
            return me.filter(item);
        };
        me.detailsManager = new JMSLib.DetailManager(2, 'logRowDetails');

        // Auswahl vorbereiten
        var filter = $('input[type="checkbox"]');
        filter.button();
        filter.click(function () {
            me.refresh();
        });

        // Geräte ermitteln
        VCRServer.ProfileCache.load().done(function (profiles) {
            var list = $('#selProfile');

            // Alle Namen eintragen
            $.each(profiles, function (index, profile) {
                list.append(new Option(profile.name));
            });

            // Auf Änderungen der Liste reagieren
            list.change(function () {
                me.reload();
            });

            // Und erstmalig laden - erst danach zeigen wir dem Anwender etwas
            me.reload(me.registerAsyncCall());

            profilesLoaded();
        });
    };
    return logPage;
})(Page);
//# sourceMappingURL=vcrnet.js.map
