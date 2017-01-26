/// <reference path='vcrserver.ts' />
/// <reference path='jmslib.ts' />

// Alle CSS Klassen, die vom Code aus gesetzt werden
class CSSClass {
    // Markiert im Aufzeichnungsplan inaktive Aufzeichnungsoptionen (e.g. VideoText)
    static inactiveOptionClass = 'inactiveRecordingOption';

    // Verändert kurz den Namen einer Eingabe um Änderungen anzuzeigen
    static blink = 'highlightText';

    // Markiert eine Schaltfläche für eine zusätzliche Bestätigung einer Aktion durch den Anwender
    static danger = 'danger';

    // Der Inhalt einer Hilfeseite
    static faq = 'faqContent';

    // Der Name eines Eingabefeldes
    static editLabel = 'editLabel';

    // Der Platzhalter zwischen dem Namen eines Eingabefeldes und dem Wert
    static editSpacing = 'spaceColumn';

    // In der Liste der Aufträge und Aufzeichnungen eine Zeile ohne den Namen eines Auftrags
    static noJobText = 'jobTextPlaceholder';

    // Die Anzeige des Namens eines Auftrags in der Liste der Aufträge und Aufzeichnungen
    static jobText = 'jobText';

    // Das Ende einer Aufzeichnung ist eventuell nicht wie erwünscht.
    static badEndTime = 'suspectEnd';
}

// Beschreibt einen Favoritensuche in der Programmzeitschrift
class SavedGuideQuery {
    constructor(rawQuery: SavedGuideQuery = null) {
        // Das deserialisierte Objekt sieht aus wie wir hat aber nur Eigenschaften
        if (rawQuery != null) {
            this.encryption = rawQuery.encryption || VCRServer.GuideEncryption.ALL;
            this.sourceType = rawQuery.sourceType || VCRServer.GuideSource.ALL;
            this.titleOnly = rawQuery.titleOnly;
            this.device = rawQuery.device;
            this.source = rawQuery.source;
            this.text = rawQuery.text;
        }

        // Der Einfachheit halber verwenden wir eine Umlenkung von Benutzeraktionen auf statische Methode
        this.onDelete = () => SavedGuideQuery.onDeleted(this);
        this.onExecute = () => SavedGuideQuery.onClick(this);
    }

    // Das zu berücksichtigende Gerät
    device: string;

    // Optional die Quelle
    source: string = null;

    // Der Text zur Suche in der üblichen Notation mit der Suchart als erstem Zeichen
    text: string;

    // Gesetzt, wenn nur im Titel gesucht werden soll
    titleOnly: boolean;

    // Die Art der zu berücksichtigenden Quelle
    sourceType: VCRServer.GuideSource;

    // Die Art der Verschlüsselung
    encryption: VCRServer.GuideEncryption;

    // Die Anzahl der Sendungen zur Suche
    private cachedCount: number = null;

    // Gesetzt, während die Anzahl geladen wird
    private loadingCount: boolean = false;

    // Die noch zu ladenden Zähler
    private static loadQueue: SavedGuideQuery[] = new Array();

    // Gesetzt, wenn ein Ladevorgang aktiv ist
    private static loading: boolean = false;

    // Aktiviert den nächsten Ladevorgang
    private static dispatchLoad(): void {
        if (SavedGuideQuery.loading)
            return;
        if (SavedGuideQuery.loadQueue.length < 1)
            return;

        // Auftrag entfernen
        var query = SavedGuideQuery.loadQueue.splice(0, 1)[0];

        // Filter zusammenbauen
        var filter = new GuideFilter();
        filter.typeFilter = query.sourceType;
        filter.station = query.source;
        filter.device = query.device;
        filter.title = query.text;
        filter.size = 0;
        if (!query.titleOnly)
            filter.content = query.text;

        SavedGuideQuery.loading = true;

        // Ergebnis ermitteln
        filter.count((count: number) => {
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
    }

    // Die aktuelle Anzahl von Sendungen zur Suche
    count(): any {
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
    }

    // Die gespeicherte Suche, mit der die Anzeige initialisiert werden soll.
    static guideScope: SavedGuideQuery = null;

    // Wird nach bei der Auswahl der Anzahl ausgelöst
    static onClick: (query: SavedGuideQuery) => void = null;

    // Wird nach dem Löschen ausgelöst
    static onDeleted: (query: SavedGuideQuery) => void = null;

    // Wird nach dem Laden der Anzahl der Sendungen ausgelöst
    static onCountLoaded: (query: SavedGuideQuery) => void = null;

    // Wird aufgerufen, wenn dieser Eintrag angewählt werden soll
    onExecute: () => void;

    // Wird aufgerufen, wenn dieser Eintrag gelöscht werden soll
    onDelete: () => void;

    // Stellt sicher, dass einige Eigenschaften nicht serialisiert werden
    static filterProperties(key: string, value: any): any {
        if (key == 'onExecute')
            return undefined;
        if (key == 'onDelete')
            return undefined;
        if (key == 'cachedCount')
            return undefined;
        if (key == 'loadingCount')
            return undefined;

        return value;
    }

    // Erstellt eine Textdarstellung
    displayText(): string {
        var display = 'Alle ';

        // Die besonderen Einschränkungen sind nur gültig, wenn keine Quelle ausgewählt wurde
        if ((this.source || '') == '') {
            // Verschlüsselung
            if (this.encryption == VCRServer.GuideEncryption.FREE)
                display += 'unverschlüsselten ';
            else if (this.encryption == VCRServer.GuideEncryption.PAY)
                display += 'verschlüsselten ';

            // Art
            if (this.sourceType == VCRServer.GuideSource.TV)
                display += 'Fernseh-';
            else if (this.sourceType == VCRServer.GuideSource.RADIO)
                display += 'Radio-';
        }

        display += 'Sendungen, die über das Gerät ';
        display += this.device;

        // Die Quelle ist optional
        if (this.source != null)
            if (this.source.length > 0) {
                display += ' von der Quelle ';
                display += this.source;
            }

        display += ' empfangen werden und deren Name ';
        if (!this.titleOnly)
            display += 'oder Beschreibung ';

        display += ' "';
        display += this.text.substr(1);
        display += '" ';

        // Auch wenn wir das jetzt nicht unterstützen gibt es auch die Suche nach exakter Gleichheit
        if (this.text[0] == '*')
            display += 'enthält';
        else
            display += 'ist';

        return display;
    }
}

// Die Verwaltung aller gespeicherten Suchen
class SavedGuideQueries {
    // Der Name der Ablage
    private static storeName = 'vcrnet.guidequeries';

    // Alle gespeicherten Suchen
    private static queries: SavedGuideQuery[];

    // Ermittelt alle gespeicherten Suchen
    static load(): SavedGuideQuery[] {
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
        }
        else
            cache = JSON.parse(fromStore);

        // In echte Objekte mit Methoden wandeln
        SavedGuideQueries.queries = $.map(cache, (rawQuery: SavedGuideQuery) => new SavedGuideQuery(rawQuery));

        return SavedGuideQueries.queries;
    }

    // Aktualisiert die gespeicherten Suchen
    static save() {
        VCRServer.updateSearchQueries(JSON.stringify(SavedGuideQueries.queries, SavedGuideQuery.filterProperties));
    }
}

// Repräsentiert die Klasse GuideFilter
class GuideFilter implements VCRServer.GuideFilterContract {
    // Der Name des aktuell ausgewählten Geräteprofils
    device: string = null;

    // Der Name der aktuell ausgewählten Quelle
    station: string = null;

    // Der minimale Startzeitpunkt in ISO Notation
    start: string = null;

    // Das Suchmuster für den Namen einer Sendung
    title: string = null;

    // Das Suchmuster für die Beschreibung einer Sendung
    content: string = null;

    // Die Anzahl von Sendungen pro Anzeigeseite
    size: number = 10;

    // Die aktuelle Seite
    index: number = 0;

    // Einschränkung auf die Art der Quellen
    typeFilter: VCRServer.GuideSource = VCRServer.GuideSource.ALL;

    // Einschränkung auf die Verschlüsselung der Quellen
    cryptFilter: VCRServer.GuideEncryption = VCRServer.GuideEncryption.ALL;

    // Wird aufgerufen, wenn sich etwas verändert hat
    onChange: () => void = null;

    // Die aktuelle Anmeldung zum verzögerten Aufruf
    private timeout: number = null;

    // Gesetzt, wenn der letzte Abruf zusätzliche Daten ermittelt hat
    private moreAvailable: boolean = false;

    // Meldet, ob ein Blättern nach vorne unterstützt wird
    canPageForward(): boolean { return this.moreAvailable; }

    // Deaktiviert verzögerte Anforderungen
    private cancelTimeout(): void {
        if (this.timeout == null)
            return;

        window.clearTimeout(this.timeout);

        this.timeout = null;
    }

    // Meldet Änderungen an der Konfiguration
    private fireChange(): void {
        this.cancelTimeout();

        if (this.onChange != null)
            this.onChange();
    }

    // Reagiert auf die Änderung der Benutzereinstellungen
    userProfileChanged(): void {
        var newSize = VCRServer.UserProfile.global.rowsInGuide;
        if (newSize == this.size)
            return;

        this.size = newSize;
        this.index = 0;
    }

    // Wechselt zur ersten Seite der Anzeige
    firstPage(): void {
        this.index = 0;
        this.fireChange();
    }

    // Wechselt zur nächsten Seite
    nextPage(): void {
        if (this.moreAvailable)
            this.index += 1;
        this.fireChange();
    }

    // Wechselt zur vorherigen Seite
    prevPage(): void {
        if (this.index > 0)
            this.index -= 1;
        this.fireChange();
    }

    // Ändert das aktuelle Gerät
    changeDevice(newDevice: string, newStation: string = null): void {
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
    }

    // Ändert die aktuell ausgewählten Quelle
    changeStation(newStation: string): void {
        this.station = newStation;
        this.index = 0;
        this.fireChange();
    }

    changeQuery(newText: string, withContent: boolean): void {
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
        this.timeout = window.setTimeout(() => this.fireChange(), 200);
    }

    // Ändert den Startzeitpunkt
    changeStart(newStart: string): void {
        this.start = newStart;
        this.index = 0;
        this.fireChange();
    }

    // Ändert die Art der Sender
    changeType(type: VCRServer.GuideSource): void {
        this.typeFilter = type;
        this.index = 0;
        this.fireChange();
    }

    // Ändert die Verschlüsselung
    changeEncryption(encryption: VCRServer.GuideEncryption): void {
        this.cryptFilter = encryption;
        this.index = 0;
        this.fireChange();
    }

    // Entfernt alle Suchbedingungen
    reset(): void {
        this.cryptFilter = VCRServer.GuideEncryption.ALL;
        this.typeFilter = VCRServer.GuideSource.ALL;
        this.station = null;
        this.content = null;
        this.title = null;
        this.start = null;
        this.index = 0;
        this.fireChange();
    }

    // Sorgt dafür, dass unsere internen Eigenschaften nicht zum Server gesendet werden
    private static filterProperties(key: string, value: any): any {
        return ((key == 'timeout') || (key == 'moreAvailable') || (key == 'onChange')) ? undefined : value;
    }

    // Fordert neue Daten an
    execute(whenLoaded: (items: GuideItem[]) => void): void {
        this.cancelTimeout();

        VCRServer.queryProgramGuide(this).then(data => {
            var items = $.map(data, (rawData: VCRServer.GuideItemContract) => new GuideItem(rawData));

            // Wir erhalten immer einen Eintrag mehr als angefordert, falls noch mehr Einträge existieren
            this.moreAvailable = items.length > this.size;

            whenLoaded(items.slice(0, this.size));
        });
    }

    // Fordert die Anzahl der Sendungen an
    count(whenLoaded: (count: number) => void): void {
        VCRServer.countProgramGuide(this, GuideFilter.filterProperties).done(whenLoaded);
    }

    // Die einzige Instanz dieser Klasse
    static global = new GuideFilter();
}

// Repräsentiert die Klasse JobScheduleData
class JobScheduleDataContract implements VCRServer.JobScheduleDataContract {
    constructor(job: VCRServer.EditJobContract, schedule: VCRServer.EditScheduleContract) {
        this.job = job;
        this.schedule = schedule;
    }

    // Der Auftrag
    job: VCRServer.EditJobContract;

    // Die Aufzeichnung im Auftrag
    schedule: VCRServer.EditScheduleContract;

    // Aktualisiert eine Aufzeichnung
    update(jobId: string, scheduleId: string, finishHash: string, onError: (message: string) => void): void {
        VCRServer
            .updateSchedule(jobId, scheduleId, this)
            .then(() => { VCRServer.UserProfile.global.refresh(); window.location.hash = finishHash; }, JMSLib.dispatchErrorMessage(onError));
    }
}

// Schnittstelle für alle Teilanwendungen
interface IPage {
    // Wird einmalig zur Vorbereitung der Anzeige aufgerufen
    onInitialize(): void;

    // Wird unmittelbar vor der Anzeige aufgerufen
    onShow(): void;

    // Die Liste der sichtbaren Verweise
    visibleLinks: string;

    // Die Überschrift der Seite
    title: string;
}

// Basisklasse für alle Teilanwendungen
class Page {
    // Alle noch ausstehenden asynchronen Initialisierungsaufrufe
    private pending: number = 1;

    // Verwaltet asynchrone Aktualisierungsaufrufe
    private nextPending = 2;

    // Das primäre Oberflächenelement
    private mainContent: JQuery;

    // Meldet eine asynchrone Initialisierung an
    registerAsyncCall(): () => void {
        var mask = this.nextPending;

        this.nextPending += mask;
        this.pending |= mask;

        return () => this.finishedAsyncCall(mask);
    }

    // Wird aufgerufen, wenn eine asynchrone Initialisierung abgeschlossen wurde
    private finishedAsyncCall(mask: number): void {
        // Das machen wir nur ein einziges Mal
        if (this.pending == 0)
            return;

        // Fertigen Aufruf anmelden
        this.pending &= ~mask;

        // Wenn alles bereit steht anzeigen lassen
        if (this.pending == 0)
            this.show();
    }

    // Wird einmalig zur Vorbereitung der Anzeige aufgerufen
    initialize(mainContent: JQuery): void {
        this.mainContent = mainContent;

        (<IPage><any>this).onInitialize();

        this.finishedAsyncCall(1);
    }

    // Wird unmittelbar vor der Anzeige aufgerufen
    onShow(): void { }

    // Wird einmalig zur Anzeige aufgerufen
    show(): void {
        var page = <IPage><any>this;

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
    }

    // Lädt die Daten zu allen Geräten
    loadProfiles(target: JQuery): void {
        var whenDone = this.registerAsyncCall();

        VCRServer.ProfileCache.getPromise().then(profiles => {
            $.each(profiles, (index: number, profile: VCRServer.ProfileInfoContract) => {
                target.append(new Option(profile.name));

                VCRServer.ProfileSourcesCache.getPromise(profile.name).then(sources => this.registerAsyncCall());
            });

            whenDone();
        });
    }
}

// Globale Initialisierungen
if (document.location.pathname !== "/default.html")
    $(() => {
        // Benutzereinstellungen einmalig anfordern
        VCRServer.UserProfile.global.refresh();

        // Informationsdaten ermitteln
        VCRServer.getServerVersion().then(data => $('#masterTitle').text('VCR.NET Recording Service ' + data.version));

        // Hier kommt der Inhalt hin
        var mainContent = $('#mainArea');
        var tagClass = '.initialDisable';

        // Alles, was unsichtbar ist, zusätzlich markieren
        $('.' + JMSLib.CSSClass.invisible).addClass(tagClass.substr(1));

        // Eine Teilanwendung laden
        function loadMain(template: string): void {
            // Zustand zurück setzen - es ist teilweise einfach, an globale statische Methoden zu binden, wenn die zugehörigen Daten auch global gehalten werden
            VCRServer.UserProfile.global.register(null);
            SavedGuideQuery.onCountLoaded = null;
            GuideFilter.global.onChange = null;
            SavedGuideQuery.onDeleted = null;
            SavedGuideQuery.onClick = null;

            // Anwender informieren und Seite anfordern
            $('#headline').text('(bitte einen Moment Geduld)');

            JMSLib.TemplateLoader.loadAbsolute('ui/' + template + '.html').done((wholeFile: string) => {
                var content = $(wholeFile).find('#mainContent');
                var starterClass = template + 'Page';
                var starter: Page = new window[starterClass];

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
        window.onhashchange = (ev: Event) => {
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
        }

        // Aktuelle Seite laden
        window.onhashchange(null);
    })

// Verwaltet die Auswahl einer Quelle für die Aufzeichnung, einschließlich der Aufzeichnungsoptionen
class SourceSelector {
    constructor(loader: SourceSelectorLoader, sibling: JQuery, pure: boolean) {
        // Erst einmal eine Kopie aus den HTML Vorlagen erstellen
        this.loader = loader;
        this.options = loader.optionTemplate.clone();
        this.source = loader.sourceTemplate.clone();

        // Diese Kopien an der gewünschten Stelle in der Oberfläche einblenden
        if (pure)
            this.source.find('.' + CSSClass.editLabel + ', .' + CSSClass.editSpacing).remove();
        else
            this.options.insertAfter(sibling);
        this.source.insertAfter(sibling);

        // Alles, was wir öfter brauchen werden, einmal nachschlagen
        this.encryptionMode = this.source.find('#filterEncryption');
        this.sourceSelectionList = this.source.find('#selSource');
        this.sourceNameField = this.source.find('#sourceName');
        this.nameMode = this.source.find('#filterName');
        this.typeMode = this.source.find('#filterType');

        // Kleine Hilfe
        var refresh = () => this.load();

        // Alle Änderungen überwachen
        this.sourceSelectionList.change(() => { this.sourceNameField.val(this.sourceSelectionList.val()); this.sourceNameField.change(); });
        this.sourceNameField.change(() => {
            var selectedSource: string = this.sourceNameField.val();

            this.sourceSelectionList.val(selectedSource);
            this.options.find('input').prop('disabled', selectedSource.length < 1);
        });
        this.encryptionMode.change(refresh);
        this.typeMode.change(refresh);
        this.nameMode.change(refresh);
    }

    // Die Arbeitsumgebung.
    private loader: SourceSelectorLoader;

    // Der Bereich mit den Optionen für die Aufzeichnungen.
    private options: JQuery;

    // Der Bereich mit der Auswahl der Quellen.
    private source: JQuery;

    // Die Liste mit der Auswahl der Quellen.
    private sourceSelectionList: JQuery;

    // Das verborgene Feld zur Übertragung der ausgewählten Quelle an das Modell.
    private sourceNameField: JQuery;

    // Die Auswahlliste mit dem Namensfilter.
    private nameMode: JQuery;

    // Die Auswahl für die Art der Quelle.
    private typeMode: JQuery;

    // Die Auswahl der Verschlüsselung der Quelle.
    private encryptionMode: JQuery;

    // Aktualisiert die Auswahlliste der Quellen.
    private load(firstCall: boolean = false): void {
        // Auswahllisten einblenden
        this.typeMode.removeClass(JMSLib.CSSClass.invisible);
        this.encryptionMode.removeClass(JMSLib.CSSClass.invisible);

        // Liste der Quellen ermitteln
        var profile = this.loader.profileSelector.val();
        var sources: VCRServer.SourceEntry[] = []; // VCRServer.ProfileSourcesCache.readCache(profile);

        // Aktuelle Quelle sichern - diese kommt unabhängig vom Filter immer in die Liste
        var currentSource = this.sourceNameField.val();

        // Filter ermitteln
        var nameFilterMode: string = this.nameMode.val();
        var nameFilter: (source: VCRServer.SourceEntry) => boolean;
        var filterOutTelevision;
        var filterOutEncrypted;

        switch (this.encryptionMode.val()) {
            case 'P': filterOutEncrypted = false; break;
            case 'F': filterOutEncrypted = true; break;
        }
        switch (this.typeMode.val()) {
            case 'T': filterOutTelevision = false; break;
            case 'R': filterOutTelevision = true; break;
        }

        if (nameFilterMode == '*')
            nameFilter = (source: VCRServer.SourceEntry) => true;
        else if (nameFilterMode == '!')
            nameFilter = (source: VCRServer.SourceEntry) => {
                var first = source.firstNameCharacter;

                if (first < '0')
                    return true;
                else if (first > 'Z')
                    return true;
                else
                    return (first > '9') && (first < 'A');
            };
        else if (nameFilterMode.length == 2)
            nameFilter = (source: VCRServer.SourceEntry) => {
                var first = source.firstNameCharacter;

                return (first >= nameFilterMode[0]) && (first <= nameFilterMode[1]);
            };
        else {
            // Immer alle Quellen anzeigen
            filterOutTelevision = undefined;
            filterOutEncrypted = undefined;

            // Zugehörige Auswahllisten ausblenden
            this.typeMode.addClass(JMSLib.CSSClass.invisible);
            this.encryptionMode.addClass(JMSLib.CSSClass.invisible);

            nameFilter = (source: VCRServer.SourceEntry) => VCRServer.UserProfile.global.recentSources[source.name];
        }

        // Zurück auf den Anfang der Liste
        this.sourceSelectionList.children().remove();
        this.sourceSelectionList.append(new Option('(Keine Quelle)', ''));

        // Liste der Quellen gemäß Filter füllen
        $.each(sources, (index: number, source: VCRServer.SourceEntry) => {
            if (source.name != currentSource) {
                if (source.isEncrypted == filterOutEncrypted)
                    return;
                if (source.isTelevision == filterOutTelevision)
                    return;
                if (!nameFilter(source))
                    return;
            }

            this.sourceSelectionList.append(new Option(source.name));
        });

        // Auswahl aktualisieren
        this.sourceNameField.change();
        this.sourceSelectionList.change();

        // Beim ersten Aufruf sind wir fertig
        if (firstCall)
            return;

        // Das Label zur Auswahl der Quellen
        var label = this.source.children().first();

        // Etwas Feedback über die (eventuell nicht optisch sichtbare) Änderung der Auswahl geben
        label.addClass(CSSClass.blink);

        // Nach kurzer Zeit zurück auf den Normalzustand
        setTimeout(() => label.removeClass(CSSClass.blink), 250);
    }

    // Setzt die Auswahl der Quelle zurück.
    reset(): void {
        this.sourceSelectionList.val(null);
    }

    // Bereitet die Nutzung der Auswahl einmalig vor, nachdem alle benötigten Elemente für die Anzeige der Oberfläche geladen wurden.
    initialize(): void {
        // Einstellung des Anwenders auslesen und in die Oberfläche übertragen
        this.encryptionMode.val(VCRServer.UserProfile.global.defaultEncryption);
        this.typeMode.val(VCRServer.UserProfile.global.defaultType);
        if (!VCRServer.UserProfile.global.hasRecentSources) {
            // Es gibt gar keine Liste von zuletzt verwendeten Quellen
            this.nameMode.children().first().remove();
            this.nameMode.children().last().attr('selected', 'selected');
        }

        // Auswahl am Geräteprofil überwachen
        this.loader.profileSelector.change(() => this.load());

        // Erstmalig die Liste der Quellen laden
        this.load(true);
    }
}

// Verwaltet die Erzeugung der Quellenauswahl
class SourceSelectorLoader {
    constructor(profile: JQuery) {
        this.profileSelector = profile;
    }

    // Die Vorlage für die besonderen Einstellungen einer Aufzeichnung.
    optionTemplate: JQuery;

    // Die Vorlage für die Auswahl der Quellen.
    sourceTemplate: JQuery;

    // Das Eingabeelement, über das die Auswahl des Gerätes erfolgt.
    profileSelector: JQuery;

    // Fordert die HTML Vorlagen vom Server an.
    loadTemplates(success: () => void): void {
        JMSLib.TemplateLoader.load('sourceSelection').done((template: string) => {
            var templateDom = $(template);

            this.optionTemplate = templateDom.find('#options');
            this.sourceTemplate = templateDom.find('#source');

            success();
        });
    }

    // Bereitet die Oberfläche für die Auswahl der Quelle vor.
    appendAfter(sibling: JQuery, pure: boolean = false): SourceSelector {
        return new SourceSelector(this, sibling, pure);
    }
}

// Algorithmen zur Prüfung der Eingaben der Benutzereinstellungen
class UserSettingsValidator implements JMSLib.IValidator {
    constructor(send: JQuery) {
        // Wir arbeiten immer auf einer Kopie
        var clone = VCRServer.UserProfile.global.clone();

        this.sendButton = send;
        this.model = clone;

        // Overfläche vorbereiten
        send.button();
        send.click(() => {
            send.button('option', 'disabled', true);

            clone.update((error: string) => JMSLib.Bindings.setErrorIndicator(send, error));
        });
    }

    // Die Schaltfläche zur Aktualisierung
    private sendButton: JQuery;

    // Die aktuellen Eingabedaten
    model: VCRServer.UserProfile;

    // Die Repräsentation der aktuellen Daten in der Oberfläche
    view: JQuery;

    // Gesetzt, wenn die zugehörige Eigenschaft im Modell ungrültig ist
    planDaysToShow: string;

    // Gesetzt, wenn die zugehörige Eigenschaft im Modell ungrültig ist
    maximumRecentSources: string;

    // Gesetzt, wenn die zugehörige Eigenschaft im Modell ungrültig ist
    guidePreTime: string;

    // Gesetzt, wenn die zugehörige Eigenschaft im Modell ungrültig ist
    guidePostTime: string;

    // Gesetzt, wenn die zugehörige Eigenschaft im Modell ungrültig ist
    rowsInGuide: string;

    // Prüft die Konsistenz von Eingabedaten
    validate(): void {
        var profile = this.model;

        // Prüfen
        this.maximumRecentSources = JMSLib.Bindings.checkNumber(profile.maximumRecentSources, 1, 50);
        this.planDaysToShow = JMSLib.Bindings.checkNumber(profile.planDaysToShow, 1, 50);
        this.guidePostTime = JMSLib.Bindings.checkNumber(profile.guidePostTime, 0, 240);
        this.guidePreTime = JMSLib.Bindings.checkNumber(profile.guidePreTime, 0, 240);
        this.rowsInGuide = JMSLib.Bindings.checkNumber(profile.rowsInGuide, 10, 100);

        // Ergebnis der Prüfung
        var isValid =
            (this.planDaysToShow == null) &&
            (this.maximumRecentSources == null) &&
            (this.guidePreTime == null) &&
            (this.guidePostTime == null) &&
            (this.rowsInGuide == null);

        // Schaltfläche anpassen
        this.sendButton.button('option', 'disabled', !isValid);
    }
}

// Beschreibt einen einzelnen Eintrag der Programmzeitschrift, der zur Anzeige vorbereitet wurde
class GuideItem implements JMSLib.IGuideItem {
    constructor(rawData: VCRServer.GuideItemContract) {
        // Zeiten umrechnen
        var duration = rawData.duration * 1000;
        var start = new Date(rawData.start);
        var end = new Date(start.getTime() + duration);

        // Daten aus der Rohdarstellung in das Modell kopieren
        this.displayDuration = JMSLib.DateFormatter.getDuration(new Date(duration));
        this.inactiveClass = rawData.active ? '' : JMSLib.CSSClass.invisible;
        this.displayStart = JMSLib.DateFormatter.getStartTime(start);
        this.displayEnd = JMSLib.DateFormatter.getEndTime(end);
        this.short = rawData.shortDescription;
        this.language = rawData.language;
        this.long = rawData.description;
        this.station = rawData.station;
        this.name = rawData.name;
        this.duration = duration;
        this.id = rawData.id;
        this.start = start;
        this.end = end;

        // Listen
        var categories = rawData.categories;
        var ratings = rawData.ratings;

        this.categories = categories.join(' ');
        this.ratings = ratings.join(' ');

        // Detailanzeige immer aktivieren
        this.showDetails = (origin: Event) => this.onShowDetails(this, origin.currentTarget);

        // Suche immer aktivieren
        this.findInGuide = () => {
            var filter = GuideFilter.global;

            // Ganz von vorne
            filter.reset();

            // Textsuche auf den Namen auf der selben Karte
            filter.device = this.id.split(':')[1];
            filter.title = '=' + this.name;
            filter.station = this.station;

            // Aufrufen
            if (window.location.hash == '#guide')
                window.location.hash = '#guide;auto';
            else
                window.location.hash = '#guide';
        };
    }

    // Wird aufgerufen, wenn der Anwender die Detailanzeige aktiviert hat
    onShowDetails: (item: GuideItem, origin: any) => void = (item: GuideItem, origin: any) => { };

    // Die eindeutige Kennung des Eintrags
    id: string;

    // Der Name der Sendung
    name: string;

    // Die Quelle
    station: string;

    // Die Sprache der Sendung
    language: string;

    // Die ausführliche Beschreibung
    long: string;

    // Die Kurzbeschreibung
    short: string;

    // Die Liste der Kategorien
    categories: string;

    // Die Liste der Freigaben
    ratings: string;

    // Der Zeitpunkt, an dem die Sendung beginnt
    start: Date;

    // Der Zeitpunkt, an dem die Sendung beginnt, aufbereitet für die Anzeige
    displayStart: string;

    // Der Zeitpunkt, an dem die Sendung endet
    end: Date;

    // Der Zeitpunkt, an dem die Sendung endet, aufbereitet für die Anzeige
    displayEnd: string;

    // Die Dauer der Sendung in Sekunden
    duration: number;

    // Die Dauer der Sendung in Sekunden, aufbereitet für die Anzeige
    displayDuration: string;

    // Die CSS Klasse die verwendet wird, um eine bereits verstrichene Sendung zu kennzeichnen
    inactiveClass: string;

    // Wird an eine Methode gebunden, die onShowDetails() aufruft
    showDetails: (origin: Event) => void;

    // Wird aufgerufen, um nach ähnlichen Sendungen in der Programmzeitschrift zu suchen
    findInGuide: () => void;

    // Eine CSS Klasse, die den Überlapp einer Aufzeichnung mit dem Eintrag ausdrückt.
    overlapClass: string = JMSLib.CSSClass.partialRecord;
}

// Verwaltet einen Eintrag aus der Programmzeitschrift und stellt sicher, dass dieser nur einmal angefordert wird
class GuideItemCache {
    private guideItem: GuideItem;

    request(device: string, source: string, start: Date, end: Date, dataAvailable: (data: GuideItem) => void): void {
        // Haben wir schon
        if (this.guideItem == undefined)
            VCRServer.getGuideItem(device, source, start, end).done((rawData: VCRServer.GuideItemContract) => {
                if (rawData == null)
                    this.guideItem = null;
                else
                    dataAvailable(this.guideItem = new GuideItem(rawData));
            });
        else if (this.guideItem != null)
            dataAvailable(this.guideItem);
    }
}

// Beschreibt einen einzelnen Eintrag in Aufzeichnungsplan
class PlanEntry {
    constructor(rawData: any) {
        // Zeiten umrechnen
        var duration = rawData.duration * 1000;
        var start = new Date(rawData.start);
        var end = new Date(start.getTime() + duration);

        // Daten aus der Rohdarstellung in das Modell kopieren
        this.station = (rawData.station == null) ? '(unbekannt)' : rawData.station;
        this.profile = (rawData.device == null) ? '' : rawData.device;
        this.displayStart = JMSLib.DateFormatter.getStartTime(start);
        this.displayEnd = JMSLib.DateFormatter.getEndTime(end);
        this.epgProfile = rawData.epgDevice;
        this.fullName = rawData.name;
        this.source = rawData.source;
        this.legacyId = rawData.id;
        this.start = start;
        this.end = end;

        // Aufzeichnungsoptionen
        this.currentGuide = rawData.epgCurrent ? '' : CSSClass.inactiveOptionClass;
        this.allAudio = rawData.allAudio ? '' : CSSClass.inactiveOptionClass;
        this.hasGuideEntry = rawData.epg ? '' : CSSClass.inactiveOptionClass;
        this.subTitles = rawData.dvbsub ? '' : CSSClass.inactiveOptionClass;
        this.videoText = rawData.ttx ? '' : CSSClass.inactiveOptionClass;
        this.dolby = rawData.ac3 ? '' : CSSClass.inactiveOptionClass;

        // Für Aufgaben konfigurieren wir keine Verweise
        if (this.station == 'PSI')
            return;
        if (this.station == 'EPG')
            return;

        // Aufzeichungsmodus ermitteln
        if (rawData.lost)
            this.mode = 'lost';
        else if (rawData.late)
            this.mode = 'late';
        else
            this.mode = 'intime';

        // Detailanzeige immer aktivieren
        this.showDetails = (origin: Event) => this.onShowDetails(this, origin.currentTarget);
        this.detailsLink = 'javascript:void(0)';

        // Bearbeiten aktivieren
        if (this.legacyId != null)
            this.editLink = '#edit;id=' + this.legacyId;

        // Abruf der Programmzeitschrift vorbereiten
        if (rawData.epg) {
            this.showGuide = (origin: Event) => this.onShowGuide(this, origin.currentTarget);
            this.guideLink = 'javascript:void(0)';
        }

        // Ausnahmen auswerten
        if (rawData.exception != null) {
            this.exceptionInfo = new PlanException(rawData.exception);

            this.exceptionMode = this.exceptionInfo.isEmpty() ? 'exceptOff' : 'exceptOn';
            this.showExceptions = (origin: Event) => this.onException(this, origin.currentTarget);
        }

        // Die Endzeit könnte nicht wie gewünscht sein
        if (rawData.suspectEndTime)
            this.endTimeSuspect = CSSClass.badEndTime;
    }

    // Wird aufgerufen, wenn der Anwender die Detailanzeige aktiviert hat.
    onShowDetails: (item: PlanEntry, origin: any) => void = (item: PlanEntry, origin: any) => { };

    // Wird aufgerufen, wenn der Anwender die erweiterten Informationen der Programmzeitschrift abruft.
    onShowGuide: (item: PlanEntry, origin: any) => void = (item: PlanEntry, origin: any) => { };

    // Wird aufgerufen, wenn der Anwender die Ausnahmen konfigurieren möchte.
    onException: (item: PlanEntry, origin: any) => void = (item: PlanEntry, origin: any) => { };

    // Der Name einer CSS Klasse zur Kennzeichnung von Aufzeichnungen über die Zeitumstellung hinweg
    endTimeSuspect: string;

    // Die Kennung der zugehörigen Quelle.
    private source: string;

    // Kennung einer Aufzeichnung, so wie sie in der ursprünglichen ASP.NET Anwendung verwendet wird.
    private legacyId: string;

    // Der volle Name des Senders.
    station: string;

    // Das DVB.NET Gerät, das die Aufzeichnung ausführen wird.
    profile: string;

    // Das DVB.NET Gerät, über das Sendungsinformationen nachgeschlagen werden können.
    private epgProfile: string;

    // Der volle Name der Aufzeichnung.
    fullName: string;

    // Der Zeitpunkt, an dem die Aufzeichnung beginnen wird.
    start: Date;

    // Der Zeitpunkt, an dem die Aufzeichnung enden wird.
    end: Date;

    // Der Startzeitpunkt formatiert für die Darstellung.
    displayStart: string;

    // Der Endzeitpunkt, formatiert für die Darstellung - es werden nur Stunden und Minuten angezeigt.
    displayEnd: string;

    // Eine CSS Markierungsklasse, wenn NICHT alle Tonspuren aufgezeichnet werden sollen.
    allAudio: string;

    // Eine CSS Markierungsklasse, wenn KEINE Dolby Digital Tonspur aufgezeichnet werden soll.
    dolby: string;

    // Eine CSS Markierungsklasse, wenn der Videotext NICHT aufgezeichnet werden soll.
    videoText: string;

    // Eine CSS Markierungsklasse, wenn DVB Untertitel NICHT aufgezeichnet werden sollen.
    subTitles: string;

    // Eine CSS Markierungsklasse, wenn die Aufzeichnung KEINE Informationen zu den Sendungen enthält.
    currentGuide: string;

    // Ein Kürzel für die Qualität der Aufzeichnung, etwa ob dieser verspätet beginnt.
    mode: string;

    // Die zugehörige Ausnahmeregel.
    exceptionInfo: PlanException;

    // Ein Kürzel für die Existenz von Ausnahmen.
    exceptionMode: string;

    // Aktiviert den Verweis zur Detailanzeige.
    detailsLink: string;

    // Wird an eine Methode gebunden, die onShowDetails() aufruft.
    showDetails: (origin: Event) => void;

    // Aktiviert den Verweis zur Bearbeitung.
    editLink: string;

    // Eine CSS Markierungsklasse, wenn KEINE Informationen der Programmzeitschrift abgerufen werden können.
    hasGuideEntry: string;

    // Aktiviert den Verweis zum Abruf der Informationen aus der Programmzeitschrift.
    guideLink: string;

    // Wird an eine Methode gebunden, die onShowGuide aufruft.
    showGuide: (origin: Event) => void;

    // Wird an eine Methode gebunden, die onExceptions aufruft.
    showExceptions: (origin: Event) => void;

    // Die zugehörigen Informationen der Programmzeitschrift.
    private guideItem = new GuideItemCache();

    // Fordert die Informationen der Programmzeitschrift einmalig an und liefert das Ergebnis bei Folgeaufrufen.
    requestGuide(dataAvailable: (data: GuideItem) => void): void {
        this.guideItem.request(this.epgProfile, this.source, this.start, this.end, dataAvailable);
    }

    // Sendet die aktuelle Ausnahmeregel zur Änderung an den VCR.NET Recording Service.
    updateException(onSuccess: () => void): void { this.exceptionInfo.update(this.legacyId, onSuccess); }
}

// Beschreibt einen einzelne Ausnahmeregel
class PlanException {
    constructor(rawData: VCRServer.PlanExceptionContract) {
        // Daten aus den Rohdaten übernehmen
        this.referenceDayDisplay = parseInt(<string>rawData.referenceDayDisplay, 10);
        this.originalStart = new Date(<string>rawData.originalStart);
        this.originalDuration = rawData.originalDuration;
        this.referenceDay = rawData.referenceDay;
        this.durationDelta = rawData.timeDelta;
        this.startDelta = rawData.startShift;
        this.rawException = rawData;
    }

    // Die Ausnahme, so wie der Web Service sie uns gemeldet hat.
    rawException: any;

    // Das Referenzdatum für die Ausnahme.
    private referenceDay: string;

    // Das Referenzdatum für die Ausnahme.
    private referenceDayDisplay: number;

    // Der ursprüngliche Startzeitpunkt.
    private originalStart: Date;

    // Die geplante Dauer der Aufzeichnung.
    originalDuration: number;

    // Die Verschiebung des Startzeitpunktes in Minuten.
    startDelta: number;

    // Die Veränderung der Dauer in Minuten.
    durationDelta: number;

    // Gesetzt, wenn diese Ausnahme aktiv ist.
    isActive: boolean = true;

    // Meldet die aktuelle Startzeit.
    private start(): Date {
        return new Date(this.originalStart.getTime() + 60 * this.startDelta * 1000);
    }

    // Meldet die aktuelle Endzeit.
    private end(): Date {
        return new Date(this.start().getTime() + 60 * (this.originalDuration + this.durationDelta) * 1000);
    }

    // Prüft, ob eine Ausnahme definiert ist.
    isEmpty(): boolean {
        return (this.startDelta == 0) && (this.durationDelta == 0);
    }

    // Meldet den Referenztag als Zeichenkette.
    displayDay(): string {
        return JMSLib.DateFormatter.getStartDate(new Date(this.referenceDayDisplay));
    }

    // Meldet die aktuelle Startzeit als Zeichenkette.
    displayStart(): string {
        return JMSLib.DateFormatter.getStartTime(this.start());
    }

    // Meldet die aktuelle Endzeit als Zeichenkette
    displayEnd(): string {
        return JMSLib.DateFormatter.getEndTime(this.end());
    }

    // Meldet die aktuelle Dauer
    displayDuration(): number {
        return this.originalDuration + this.durationDelta;
    }

    isDurationDeltaValid(): boolean {
        return (this.durationDelta > -this.originalDuration);
    }

    // Sendet die aktuellen Daten der Ausnahme an den VCR.NET Recording Service.
    update(planEntryLegacyId: string, onSuccess: () => void): void {
        VCRServer.updateException(planEntryLegacyId, this.referenceDay, this.startDelta, this.durationDelta).then(onSuccess);
    }

    // Bereitet das Formular für Änderungen vor.
    startEdit(item: PlanEntry, element: JQuery, reload: () => void): void {
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

        // Konfigurieren
        durationSlider.slider(options);
        startSlider.slider(options);
        disableButton.button();
        zeroButton.button();
        sendButton.button();

        // Initialwerte setzen
        startSlider.slider('value', this.startDelta);
        durationSlider.slider('value', this.durationDelta);

        // Aktualisierung
        var refresh = () => { JMSLib.HTMLTemplate.applyTemplate(this, element); return true; };

        // Anmelden
        startSlider.slider({
            slide: (slider: any, newValue: any) => {
                this.startDelta = newValue.value;
                return refresh();
            }
        });
        durationSlider.slider({
            slide: (slider: any, newValue: any) => {
                // Das geht nicht
                if (this.originalDuration + newValue.value < 0)
                    return false;

                this.durationDelta = newValue.value;
                return refresh();
            }
        });
        zeroButton.click(() => {
            // Ausnahme entfernen
            durationSlider.slider('value', this.durationDelta = 0);
            startSlider.slider('value', this.startDelta = 0);

            refresh();
        });
        disableButton.click(() => {
            // Ausnahme entfernen
            durationSlider.slider('value', this.durationDelta = -this.originalDuration);

            refresh();
        });
        sendButton.click(() => {
            // Das geht nur einmal
            sendButton.button('disable', true);

            // Durchreichen
            item.updateException(reload);
        });
    }
}

// Beschreibt die Daten einer Aufzeichnung
class ScheduleData {
    constructor(existingData: VCRServer.JobScheduleInfoContract) {
        // Schauen wir mal, ob wir geladen wurden
        if (existingData != null) {
            // Aufzeichnungsdaten prüfen
            var rawData = existingData.schedule;
            if (rawData != null) {
                var repeat = rawData.repeatPattern;
                var start = new Date(rawData.firstStart);
                var end = new Date(start.getTime() + 60000 * rawData.duration);

                // Übernehmen
                this.exceptionInfos = $.map(rawData.exceptions, (rawException: VCRServer.PlanExceptionContract) => new PlanException(rawException));
                this.lastDay = (repeat == 0) ? ScheduleData.maximumDate : new Date(rawData.lastDay);
                this.firstStart = new Date(start.getFullYear(), start.getMonth(), start.getDate());
                this.repeatWednesday = (repeat & ScheduleData.flagWednesday) != 0;
                this.repeatThursday = (repeat & ScheduleData.flagThursday) != 0;
                this.repeatSaturday = (repeat & ScheduleData.flagSaturday) != 0;
                this.repeatTuesday = (repeat & ScheduleData.flagTuesday) != 0;
                this.repeatMonday = (repeat & ScheduleData.flagMonday) != 0;
                this.repeatFriday = (repeat & ScheduleData.flagFriday) != 0;
                this.repeatSunday = (repeat & ScheduleData.flagSunday) != 0;
                this.startTime = JMSLib.DateFormatter.getEndTime(start);
                this.endTime = JMSLib.DateFormatter.getEndTime(end);
                this.withSubtitles = rawData.withSubtitles;
                this.withVideotext = rawData.withVideotext;
                this.allLanguages = rawData.allLanguages;
                this.includeDolby = rawData.includeDolby;
                this.sourceName = rawData.sourceName,
                    this.id = existingData.scheduleId;
                this.name = rawData.name;

                // Fertig
                return;
            }
        }

        var now = Date.now();
        var nowDate = new Date(now);

        // Eine ganz neue Aufzeichnung.
        this.firstStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate());
        this.endTime = JMSLib.DateFormatter.getEndTime(new Date(now + 7200000));
        this.withSubtitles = VCRServer.UserProfile.global.defaultDVBSubtitles;
        this.allLanguages = VCRServer.UserProfile.global.defaultAllLanguages;
        this.withVideotext = VCRServer.UserProfile.global.defaultVideotext;
        this.includeDolby = VCRServer.UserProfile.global.defaultDolby;
        this.startTime = JMSLib.DateFormatter.getEndTime(nowDate);
        this.lastDay = ScheduleData.maximumDate;
        this.repeatWednesday = false;
        this.repeatThursday = false;
        this.repeatSaturday = false;
        this.repeatTuesday = false;
        this.repeatMonday = false;
        this.repeatFriday = false;
        this.repeatSunday = false;
        this.sourceName = '';
        this.name = '';
    }

    // Der kleinste erlaubte Datumswert.
    static minimumDate: Date = new Date(1963, 8, 29);

    // Der höchste erlaubte Datumswert.
    static maximumDate: Date = new Date(2999, 11, 31);

    // Das Bit für Montag.
    static flagMonday: number = 0x01;

    // Das Bit für Dienstag.
    static flagTuesday: number = 0x02;

    // Das Bit für Mittwoch.
    static flagWednesday: number = 0x04;

    // Das Bit für Donnerstag.
    static flagThursday: number = 0x08;

    // Das Bit für Freitag.
    static flagFriday: number = 0x10;

    // Das Bit für Samstag.
    static flagSaturday: number = 0x20;

    // Das Bit für Sonntag.
    static flagSunday: number = 0x40;

    // Die eindeutige Kennung der Aufzeichnung.
    id: string;

    // Der Name der Aufzeichnung.
    name: string;

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

    // Das Startdatum der Aufzeichnung.
    firstStart: Date;

    // Die Startzeit der Aufzeichnung.
    startTime: string;

    // Die Endzeit der Aufzeichnung.
    endTime: string;

    // Der Zeitpunkt der letzten Aufzeichnung.
    lastDay: Date;

    // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
    repeatMonday: boolean;

    // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
    repeatTuesday: boolean;

    // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
    repeatWednesday: boolean;

    // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
    repeatThursday: boolean;

    // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
    repeatFriday: boolean;

    // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
    repeatSaturday: boolean;

    // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
    repeatSunday: boolean;

    // Die zugehörige Ausnahmeregel.
    exceptionInfos: PlanException[] = new Array();

    // Meldet das Wiederholungsmuster.
    repeatPattern(): number {
        var pattern =
            (this.repeatMonday ? ScheduleData.flagMonday : 0) |
            (this.repeatTuesday ? ScheduleData.flagTuesday : 0) |
            (this.repeatWednesday ? ScheduleData.flagWednesday : 0) |
            (this.repeatThursday ? ScheduleData.flagThursday : 0) |
            (this.repeatFriday ? ScheduleData.flagFriday : 0) |
            (this.repeatSaturday ? ScheduleData.flagSaturday : 0) |
            (this.repeatSunday ? ScheduleData.flagSunday : 0);

        return pattern;
    }

    // Erstellt eine für die Datenübertragung geeignete Variante.
    toWebService(): VCRServer.EditScheduleContract {
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

        $.each(this.exceptionInfos, (index: number, info: PlanException) => {
            if (info.isActive)
                exceptions.push(info.rawException);
        });

        // Fertig
        var contract: VCRServer.EditScheduleContract = {
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
            name: this.name,
        };

        // Report
        return contract;
    }
}

// Algorithmen zur Prüfung der Eingaben einer Aufzeichnung
class ScheduleDataValidator implements JMSLib.IValidator {
    constructor(job: JobDataValidator, existingData: VCRServer.JobScheduleInfoContract) {
        this.model = new ScheduleData(existingData);
        this.job = job;

        job.schedules.push(this);
    }

    // Der zugehlörige Auftrag.
    private job: JobDataValidator;

    // Das aktuell zugehörige Oberflächenelement.
    view: JQuery;

    // Die eigentlichen Daten.
    model: ScheduleData;

    // Die Fehlermeldung zum Namen des Auftrags.
    name: string;

    // Die Fehlermeldungen für die Tage.
    firstStart: string;
    lastDay: string;

    // Die Fehlermeldungen für die Zeiten.
    startTime: string;
    endTime: string;

    validate(): void {
        // Das sollen wir prüfen
        var schedule = this.model;
        var job = this.job.model;

        // Alles zurücksetzen
        this.firstStart = null;
        this.startTime = null;
        this.lastDay = null;
        this.endTime = null;
        this.name = null;

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
        var endDate = this.view.find('#endDateContainer');
        if (schedule.repeatPattern() == 0) {
            endDate.addClass(JMSLib.CSSClass.invisible);
        }
        else {
            endDate.removeClass(JMSLib.CSSClass.invisible);

            var lastDay = schedule.lastDay;
            if (lastDay == null)
                this.lastDay = 'Es muss ein Datum für die letzte Aufzeichnung angegeben werden';
            else if (firstDay != null)
                if (lastDay < firstDay)
                    this.lastDay = 'Das Datum der letzten Aufzeichnung darf nicht vor der ersten Aufzeichnung liegen';
        }

        // Der Auftrag muss auch geprüft werden
        if (!JMSLib.Bindings.validate(this.job))
            return;

        // Ergebnis berechnen
        var isValid = (this.firstStart == null) && (this.startTime == null) && (this.lastDay == null) && (this.endTime == null) && (this.name == null);

        // Schaltfläche deaktivieren
        if (!isValid)
            this.job.sendButton.button('option', 'disabled', true);
    }
}

// Beschreibt die Daten eines Auftrags
class JobData {
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
                this.operationType = 'Übernehmen';
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
        this.operationType = 'Anlegen';
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

    // Die Art der Operation.
    operationType: string;

    // Gesetzt, wenn nach dem Hinzufügen zur Programmzeitschrift gesprungen werden soll.
    guideAfterAdd: boolean;

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

    // Aktualisiert die Daten des Auftrags und der zugehörigen Aufzeichnung.
    createOrUpdate(schedule: ScheduleData, onError: (message: string) => void): void {
        // Wohin geht es nach dem Abschluss
        var whenFinished: string
        if (this.guideAfterAdd)
            whenFinished = '#guide';
        else
            whenFinished = '#plan';

        var data = new JobScheduleDataContract(this.toWebService(), schedule.toWebService());

        // Aktualisierungsbefehl auslösen
        data.update(this.id, schedule.id, whenFinished, onError);
    }
}

// Algorithmen zur Prüfung der Eingaben eines Auftrags
class JobDataValidator implements JMSLib.IValidator {
    constructor(existingData: VCRServer.JobScheduleInfoContract, updateButton: JQuery, defaultProfile: string) {
        this.model = new JobData(existingData, defaultProfile);
        this.sendButton = updateButton;

        // Schaltfläche aufbereiten
        updateButton.text(this.model.operationType);
        updateButton.button({ disabled: true });
        updateButton.click(() => {
            updateButton.button('option', 'disabled', true);

            // Das ist die eigentliche Aktualisierung
            this.model.createOrUpdate(this.schedules[0].model, (error: string) => {
                JMSLib.Bindings.setErrorIndicator(updateButton, error);

                this.waiting = false;
            });
        });
    }

    // Die Schaltfläche zum Übertragen der Daten an den Web Service.
    sendButton: JQuery;

    // Gesetzt, während eine Anfrage an der Web Dienst läuft.
    private waiting: boolean = false;

    // Das aktuell zugehörige Oberflächenelement.
    view: JQuery;

    // Die eigentlichen Daten.
    model: JobData;

    // Die Fehlermeldung zum Namen des Auftrags.
    name: string;

    // Die Fehlermeldung zur Auswahl der Quelle.
    sourceName: string;

    // Alle zurzeit in Bearbeitung befindlichen Aufzeichnungen dieses Auftrags.
    schedules: ScheduleDataValidator[] = new Array();

    // Alle verbotenen Zeichen.
    private static forbidenCharacters: RegExp = /[\\\/\:\*\?\"\<\>\|]/;

    static isNameValid(name: string): boolean {
        return name.search(JobDataValidator.forbidenCharacters) < 0;
    }

    validate(): void {
        // Das sollen wir prüfen
        var job = this.model;

        // Alles zurücksetzen
        JMSLib.Bindings.setErrorIndicator(this.sendButton, null);
        this.sourceName = null;
        this.name = null;

        // Name
        job.name = job.name.trim();
        if (job.name.length < 1)
            this.name = 'Ein Auftrag muss immer einen Namen haben';
        else if (!JobDataValidator.isNameValid(job.name))
            this.name = 'Der Name des Auftrags enthält ungültige Zeichen';

        // Quelle
        if (job.sourceName.length < 1)
            $.each(this.schedules, (index: number, schedule: ScheduleDataValidator) => {
                var sourceName = schedule.model.sourceName;
                if (sourceName != null)
                    if (sourceName.length > 0)
                        return true;

                this.sourceName = 'Es muss immer eine Quelle angegeben werden (auf Wunsch auch bei der Aufzeichnung)';

                return false;
            });

        // Ergebnis berechnen
        var isValid = (this.sourceName == null) && (this.name == null);

        // Schaltfläche entsprechend einstellen
        this.sendButton.button('option', 'disabled', this.waiting || !isValid);
    }

    // Bereitet das Löschen der Aufzeichnung vor - das Löschen der letzten Aufzeichnung eines Auftrags entfernt auch den Auftrag.
    prepareDelete(info: JQueryEventObject): void {
        var job = this.model;
        var schedule = this.schedules[0].model;

        var deleteButton = $(info.currentTarget);
        deleteButton.addClass(CSSClass.danger);
        deleteButton.unbind('click');

        deleteButton.click(() => {
            deleteButton.button('option', 'disabled', true);

            VCRServer
                .deleteSchedule(job.id, schedule.id)
                .done(() => window.location.href = '#plan')
                .fail((result: JQueryXHR) => {
                    var info: any = $.parseJSON(result.responseText);

                    deleteButton.addClass(JMSLib.CSSClass.invalid);
                    deleteButton.attr('title', info.ExceptionMessage);
                    deleteButton.button('option', 'disabled', false);
                });
        });
    }
}

// Eingabeelemente für die Sicherheitskonfiguration
class SecuritySettingsValidator implements JMSLib.IValidator {
    constructor(settings: VCRServer.SecuritySettingsContract) {
        this.model = settings;
    }

    // Die eigentlichen Modelldaten, die an Oberflächenelemente gebunden werden sollen
    model: VCRServer.SecuritySettingsContract;

    // Das Oberflächenelement zur Repräsentation des Modells als Ganzes
    view: JQuery;

    // Methode zur Durchführung einer Prüfung
    validate(): void { }
}

// Eingabeelemente für das Regelwerk der Aufzeichnungsplanung
class ScheduleRulesValidator implements JMSLib.IValidator {
    constructor(settings: VCRServer.SchedulerRulesContract) {
        this.model = settings;
    }

    // Die eigentlichen Modelldaten, die an Oberflächenelemente gebunden werden sollen
    model: VCRServer.SchedulerRulesContract;

    // Das Oberflächenelement zur Repräsentation des Modells als Ganzes
    view: JQuery;

    // Methode zur Durchführung einer Prüfung
    validate(): void { }
}

// Eingabeelemente für die Verzeichnisse
class DirectorySettingsValidator implements JMSLib.IValidator {
    constructor(settings: VCRServer.DirectorySettingsContract, send: JQuery) {
        this.sendButton = send;
        this.model = settings;
    }

    // Die Schaltfläche zur Übertragung der Daten.
    private sendButton: JQuery;

    // Die eigentlichen Modelldaten, die an Oberflächenelemente gebunden werden sollen
    model: VCRServer.DirectorySettingsContract;

    // Das Oberflächenelement zur Repräsentation des Modells als Ganzes
    view: JQuery;

    // Fehlermeldung für das Muster für Dateinamen.
    pattern: string;

    // Methode zur Durchführung einer Prüfung
    validate(): void {
        // Wir prüfen hier nur das Suchmuster
        var pattern = this.model.pattern;
        if ((pattern != null) && (pattern.length > 0))
            this.pattern = null;
        else
            this.pattern = 'Das Muster für Dateinamen darf nicht leer sein.';

        this.sendButton.button('option', 'disabled', this.pattern != null);
    }
}

// Eingabeelemente für die Programmzeitschrift
class GuideSettingsValidator implements JMSLib.IValidator {
    constructor(settings: VCRServer.GuideSettingsContract, send: JQuery, form: JQuery) {
        this.model = settings;
        this.sendButton = send;
        this.editForm = form;

        // Stundenliste in Markierungen umsetzen
        JMSLib.HourListSettings.decompress(settings, settings.hours);

        // Aktivierung berücksichtigen
        GuideSettingsValidator.activated(settings, settings.duration > 0);
    }

    // Liest oder setzt den Aktivierungszustand
    static activated(settings: VCRServer.GuideSettingsContract, newValue: boolean = null): any {
        if (newValue != null)
            settings['activated'] = newValue;
        else
            return settings['activated'];
    }

    // Die Schaltfläche zum Ausläsen der Änderungen
    private sendButton: JQuery;

    // Die Oberfläche zur Pflege der Daten
    private editForm: JQuery;

    // Die eigentlichen Modelldaten, die an Oberflächenelemente gebunden werden sollen
    model: VCRServer.GuideSettingsContract;

    // Das Oberflächenelement zur Repräsentation des Modells als Ganzes
    view: JQuery;

    // Die Fehlermeldung bei einer ungültigen Dauer.
    duration: string;

    // Die Fehlermeldung bei einem ungültigen Schwellenwert.
    joinHours: string;

    // Die Fehlermeldung bei einem ungültigen Intervall.
    minDelay: string;

    // Stellt sicher, dass nur bekannte Eigenschaften an den Dienst übertragen werden
    static filterPropertiesOnSend(key: string, value: any): any {
        if (key == 'sourceName')
            return undefined;
        if (key == 'activated')
            return undefined;
        if (JMSLib.HourListSettings.isHourFlag(key))
            return undefined;

        return value;
    }

    // Methode zur Durchführung einer Prüfung
    validate(): void {
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
    }
}

// Eingabeelemente für die Aktualisierung der Quellen
class SourceScanSettingsValidator implements JMSLib.IValidator {
    constructor(settings: VCRServer.SourceScanSettingsContract, send: JQuery) {
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

    // Die Schaltfläche zur Ausführung der Aktualsisierung
    private sendButton: JQuery;

    // Liest oder setzt den Aktivierungszustand
    static mode(settings: VCRServer.SourceScanSettingsContract, newValue: string = null): any {
        if (newValue != null)
            settings['mode'] = newValue;
        else
            return settings['mode'];
    }

    // Die eigentlichen Modelldaten, die an Oberflächenelemente gebunden werden sollen
    model: VCRServer.SourceScanSettingsContract;

    // Das Oberflächenelement zur Repräsentation des Modells als Ganzes
    view: JQuery;

    // Fehlermeldung für die Dauer der Aktualisierung
    duration: string;

    // Fehlermeldungen für den Zeitraum der vorgezogenen Aktualisierung
    joinDays: string;

    // Fehlermeldung für den Abstand zwischen den Aktualisierungen
    interval: string;

    // Stellt sicher, dass nur bekannte Eigenschaften an den Dienst übertragen werden
    static filterPropertiesOnSend(key: string, value: any): any {
        if (key == 'mode')
            return undefined;
        if (JMSLib.HourListSettings.isHourFlag(key))
            return undefined;

        return value;
    }

    // Methode zur Durchführung einer Prüfung
    validate(): void {
        var model = this.model;

        // Anzeige anpassen
        var mode = SourceScanSettingsValidator.mode(model);
        switch (mode) {
            case 'D': $('#scanFormPeriodic').addClass(JMSLib.CSSClass.invisible); $('#scanFormDuration').addClass(JMSLib.CSSClass.invisible); break;
            case 'M': $('#scanFormPeriodic').addClass(JMSLib.CSSClass.invisible); $('#scanFormDuration').removeClass(JMSLib.CSSClass.invisible); break;
            case 'P': $('#scanFormPeriodic').removeClass(JMSLib.CSSClass.invisible); $('#scanFormDuration').removeClass(JMSLib.CSSClass.invisible); break;
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
    }
}

// Eingabeelemente für ein einzelnes Gerät
class ProfileValidator implements JMSLib.IValidator {
    constructor(settings: VCRServer.ProfileContract, listValidator: ProfileSettingsValidator) {
        this.model = settings;
        this.list = listValidator;
    }

    // Die zugehörige Liste aller Geräte
    private list: ProfileSettingsValidator;

    // Die eigentlichen Modelldaten, die an Oberflächenelemente gebunden werden sollen
    model: VCRServer.ProfileContract;

    // Das Oberflächenelement zur Repräsentation des Modells als Ganzes
    view: JQuery;

    // Methode zur Durchführung einer Prüfung
    validate(): void { this.list.validate(); }

    // Fehlermeldung für den Aktivierungszustand
    active: string;

    // Fehlermeldung für die Prioriät.
    priority: string;

    // Fehlermeldung für die Anzahl der gleichzeitg entschlüsselbaren Quellen.
    ciLimit: string;

    // Fehlermeldung für die Anzahl der gleichzeitig empfangbaren Quellen.
    sourceLimit: string;

    // Methode zur Durchführung einer Prüfung
    doValidate(defaultProfile: string): boolean {
        var model = this.model;

        this.sourceLimit = JMSLib.Bindings.checkNumber(model.sourceLimit, 1, 32);
        this.priority = JMSLib.Bindings.checkNumber(model.priority, 0, 100);
        this.ciLimit = JMSLib.Bindings.checkNumber(model.ciLimit, 0, 16);
        this.active = null;

        if (model.name == defaultProfile)
            if (!model.active)
                this.active = 'Das bevorzugte Geräteprofil muss auch für Aufzeichnungen verwendet werden.'

        JMSLib.Bindings.synchronizeErrors(this);

        return (this.active == null) && (this.priority == null) && (this.ciLimit == null) && (this.sourceLimit == null);
    }
}

// Eingabeelemente für die Geräte
class ProfileSettingsValidator implements JMSLib.IValidator {
    constructor(settings: VCRServer.ProfileSettingsContract, send: JQuery) {
        this.model = settings;
        this.sendButton = send;
    }

    // Erzeugt die Präsentation für ein einzelnes Geräteprofil
    bindProfiles(deviceTable: JQuery, deviceTemplate: JQuery): void {
        $.each(this.model.profiles, (index: number, profile: VCRServer.ProfileContract) => {
            var view = deviceTemplate.clone();
            var validator = new ProfileValidator(profile, this);

            this.profiles.push(validator);
            deviceTable.append(view);

            JMSLib.Bindings.bind(validator, view);
        });
    }

    // Die Schaltfläche zum Übertragen der Daten
    private sendButton: JQuery;

    // Alle untergeordneten Prüfalgorithmen
    private profiles: ProfileValidator[] = new Array();

    // Die eigentlichen Modelldaten, die an Oberflächenelemente gebunden werden sollen
    model: VCRServer.ProfileSettingsContract;

    // Das Oberflächenelement zur Repräsentation des Modells als Ganzes
    view: JQuery;

    // Fehlermeldung zum bevorzugten Geräteprofil
    defaultProfile: string;

    // Methode zur Durchführung einer Prüfung
    validate(): void {
        var model = this.model;
        var defaultProfile = model.defaultProfile;
        var isValid = true;

        this.defaultProfile = null;

        // Alle Geräte individuell überprüfen
        $.each(this.profiles, (index: number, profile: ProfileValidator) => {
            if (profile.doValidate(defaultProfile))
                return;

            isValid = false;

            if (profile.active != null)
                this.defaultProfile = 'Dieses Gerät ist nicht für Aufzeichnungen vorgesehen';
        });

        JMSLib.Bindings.synchronizeErrors(this);

        this.sendButton.button('option', 'disabled', !isValid);
    }
}

// Eingabeelemente für die Sicherheitskonfiguration
class OtherSettingsValidator implements JMSLib.IValidator {
    constructor(settings: VCRServer.OtherSettingsContract, send: JQuery) {
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
    static hibernationMode(settings: VCRServer.OtherSettingsContract, newValue: string = null): any {
        if (newValue != null)
            settings['hibernate'] = newValue;
        else
            return settings['hibernate'];
    }

    // Die Schaltfläche zum Ausläsen der Aktualisierung
    private sendButton: JQuery;

    // Die eigentlichen Modelldaten, die an Oberflächenelemente gebunden werden sollen
    model: VCRServer.OtherSettingsContract;

    // Das Oberflächenelement zur Repräsentation des Modells als Ganzes
    view: JQuery;

    // Die Fehlermeldung für eine Falscheingabe bei der Verweildauer von Aufträgen im Archiv
    archive: string;

    // Die Fehlermeldung für die minimale Verweildauer im Schlafzustand
    forcedHibernationDelay: string;

    // Die Fehlermeldung für die Vorlaufzeit beim Aufwachen aus dem Schlafzustand
    hibernationDelay: string;

    // Die Fehlermeldung für eine Falscheingabe bei der Verweildauer von Protokolleinträgen
    protocol: string;

    // Die Fehlermeldung den sicheren TCP/IP Port
    sslPort: string;

    // Die Fehlermeldung den regulären TCP/IP Port
    webPort: string;

    // Methode zur Durchführung einer Prüfung
    validate(): void {
        var model = this.model;

        this.forcedHibernationDelay = JMSLib.Bindings.checkNumber(model.forcedHibernationDelay, 5, 60);
        this.hibernationDelay = JMSLib.Bindings.checkNumber(model.hibernationDelay, 0, 600);
        this.sslPort = JMSLib.Bindings.checkNumber(model.sslPort, 1, 0xffff);
        this.webPort = JMSLib.Bindings.checkNumber(model.webPort, 1, 0xffff);
        this.protocol = JMSLib.Bindings.checkNumber(model.protocol, 1, 13);
        this.archive = JMSLib.Bindings.checkNumber(model.archive, 1, 13);

        var isValid =
            (this.forcedHibernationDelay == null) &&
            (this.hibernationDelay == null) &&
            (this.protocol == null) &&
            (this.sslPort == null) &&
            (this.webPort == null) &&
            (this.archive == null);

        this.sendButton.button('option', 'disabled', !isValid);
    }
}

// Das Modell zur Anzeige einer Aktivität auf einem Gerät
class CurrentInfo {
    constructor(rawData: VCRServer.PlanCurrentContract) {
        // Sonderbehandlung für unbenutzte Geräte
        if (rawData.isIdle) {
            // Eigentlich zeigen wir nichts
            this.hideTarget = JMSLib.CSSClass.invisible;
            this.name = '(keine Aufzeichnung geplant)';
            this.device = rawData.device;
            this.mode = 'intime';

            // Fertig
            return;
        }

        // Zeiten umrechnen
        var duration = rawData.duration * 1000;
        var start = new Date(rawData.start);
        var end = new Date(start.getTime() + duration);
        var outdated = end.getTime() <= Date.now();

        // Übernehmen
        this.sourceName = (rawData.sourceName == null) ? '' : rawData.sourceName;
        this.displayStart = JMSLib.DateFormatter.getStartTime(start);
        this.originalRemainingMinutes = rawData.remainingMinutes;
        this.size = (rawData.size == null) ? '' : rawData.size;
        this.displayEnd = JMSLib.DateFormatter.getEndTime(end);
        this.scheduleIdentifier = rawData.referenceId;
        this.streamTarget = rawData.streamTarget;
        this.device = rawData.device;
        this.source = rawData.source;
        this.legacyId = rawData.id;
        this.name = rawData.name;
        this.start = start;
        this.end = end;

        // Aufzeichungsmodus ermitteln
        if (this.scheduleIdentifier != null)
            if (outdated)
                this.mode = 'null';
            else
                this.mode = 'running';
        else if (rawData.late)
            this.mode = 'late';
        else
            this.mode = 'intime';

        // Bearbeiten aktivieren
        if (this.legacyId != null)
            this.editLink = '#edit;id=' + this.legacyId;

        // Abruf der Programmzeitschrift vorbereiten
        if (!outdated)
            if (rawData.epg) {
                this.showGuide = (origin: Event) => CurrentInfo.guideDisplay(this, origin.currentTarget);
                this.guideLink = 'javascript:void(0)';
            }

        // Manipulation laufender Aufzeichnungen
        if (this.scheduleIdentifier != null) {
            this.editActive = (origin: Event) => CurrentInfo.startAbort(this, origin.currentTarget);
            this.activeLink = 'javascript:void(0)';

            if (rawData.streamIndex >= 0) {
                var url = VCRServer.getDeviceRoot() + encodeURIComponent(this.device) + '/' + rawData.streamIndex + '/';

                this.viewShift = url + 'TimeShift';
                this.viewLive = url + 'Live';
                this.hideViewer = null;
            }
        }

        // Zieladresse ausblenden
        if (this.streamTarget == null)
            this.hideTarget = JMSLib.CSSClass.invisible;
    }

    // Die eindeutige Referenz auf die Definition der Aufzeichnung.
    private legacyId: string;

    // Der vollständige Anzeigename.
    name: string;

    // Das Gerät, auf dem die Aufzeichnung ausgeführt wird oder werden soll.
    device: string;

    // Der Anzeigetext für den Startzeitpunkt.
    displayStart: string = '';

    // Der Anzeigetext für das Ende.
    displayEnd: string = '';

    // Die Art der Aufzeichnung.
    mode: string;

    // Ein vom Web Server bereitgestellte Information über die aktuelle Größe der Aufzeichnung.
    size: string = '';

    // Aktiv, wenn die Aufzeichnung verändert werden kann.
    editLink: string = null;

    // Aktiv, wenn zum Aufzeichnungszeitraum Informationen in der Programmzeitschrift existieren.
    guideLink: string;

    // Methode, die zur Anzeige der Programmzeitschrigft aufgerufen wird.
    showGuide: (origin: Event) => void;

    // Aktiv, wenn die Aufzeichnung gerade ausgeführt wird.
    activeLink: string;

    // Methode, die zur Manipulation einer laufenden Aufzeichnung aufgerufen wird.
    editActive: (origin: Event) => void;

    // Die ursprüngliche verbleibende Laufzeit der Aufzeichnung.
    originalRemainingMinutes: number;

    // Die aktuelle verbleibende Laufzeit der Aufzeichnung, so wie vom Anwender verändert.
    remainingMinutes: number;

    // Der Verweis zur LIVE Ansicht einer Aufzeichnung.
    viewLive: string;

    // Der Verweis zur zeitversetzen Ansicht einer Aufzeichnung.
    viewShift: string;

    // Die CSS Klasse zum Verbergen der Verweise zur Ansicht einer Aufzeichnung.
    hideViewer: string = JMSLib.CSSClass.invisible;

    // Das Ziel des Netzwerkversands einer Aufzeichnung.
    streamTarget: string;

    // Die CSS Klasse zum Verbergen des Ziels des Netwerkversands.
    hideTarget: string;

    // Gesetzt, wenn beim Abbrechen einer Aufzeichnung der Übergang in den Schlafzustand unterbunden werden soll.
    suppressHibernate: boolean;

    // Die eindeutige Kennung einer laufenden Aufzeichnung.
    private scheduleIdentifier: string;

    // Der aktuelle Endzeitpunkt der Aufzeichnung, so wie vom Anwender korrigiert.
    private currentEnd(): Date {
        return new Date(this.end.getTime() - (this.originalRemainingMinutes - this.remainingMinutes) * 60000);
    }

    // Der Anzeigetext für den aktuellen Endzeitpunkt.
    currentEndDisplay(): string {
        return JMSLib.DateFormatter.getEndTime(this.currentEnd());
    }

    // Der Name der zugehörigen Quelle.
    sourceName: string = '';

    // Die Kennung der zugehörigen Quelle.
    private source: string;

    // Der Zeitpunkt, an dem die Aufzeichnung beginnen wird.
    start: Date;

    // Der Zeitpunkt, an dem die Aufzeichnung enden wird.
    end: Date;

    // Die zugehörigen Informationen der Programmzeitschrift.
    private guideItem = new GuideItemCache();

    // Fordert die Informationen der Programmzeitschrift einmalig an und liefert das Ergebnis bei Folgeaufrufen.
    requestGuide(dataAvailable: (data: GuideItem) => void): void {
        this.guideItem.request(this.device, this.source, this.start, this.end, dataAvailable);
    }

    // Wird aufgerufen, wenn die Programmzeitschrift für einen Aufzeichnungszeitraum abgerufen werden soll.
    static guideDisplay: (item: CurrentInfo, origin: any) => void;

    // Wird aufgerufen, wenn der Anwender den Endzeitpunkt einer laufenden Aufzeichnung verändern möchte.
    static startAbort: (item: CurrentInfo, origin: any) => void;

    // Ruft die aktuelle Liste der Aufzeichnungen vom Web Dienst ab.
    static load(whenLoaded: (infos: CurrentInfo[]) => void): void {
        VCRServer.getPlanCurrent().then(data =>
            whenLoaded($.map(data, (rawData: any) =>
                new CurrentInfo(rawData))));
    }

    // Aktualisiert den Endzeitpunkt dieser laufenden Aufzeichnung.
    updateEndTime(whenDone: () => void): void {
        var end = (this.remainingMinutes > 0) ? this.currentEnd() : this.start;

        VCRServer.updateEndTime(this.device, this.suppressHibernate, this.scheduleIdentifier, end).done(whenDone);
    }
}

// Beschreibt eine Zeile in der Liste der Aufträge und Aufzeichnungen
interface IInfoRow {
    // Der Name eines Auftrags
    jobText: string;

    // Die Darstellungsklasse des Auftrags
    jobTextClass: string;

    // Der Name einer Aufzeichnung oder ein Sondertext zum Anlegen neuer Aufzeichnungen
    scheduleText: string;

    // Der mit dem Eintrag verbundene Verweis
    link: string;

    // Gesetzt, wenn der Auftrag noch noohc im Archiv liegt
    isActive: boolean;
}

// Stelle die Daten einer Aufzeichnung dar
class InfoSchedule implements IInfoRow {
    constructor(rawInfo: VCRServer.InfoScheduleContract, isActive: boolean) {
        // Name von Aufzeichnung und Quelle ermitteln
        var name = (rawInfo.name == '') ? 'Aufzeichnung' : rawInfo.name;
        var source = rawInfo.sourceName;

        // Zeiten ermitteln
        var start = new Date(rawInfo.start);
        var pattern = rawInfo.repeatPattern;
        var startAsString: string;

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

        this.scheduleText = name + ': ' + startAsString + ' auf ' + source;
        this.isActive = isActive;
        this.link = rawInfo.id;
    }

    // Der Name des Auftrags ist immer leer
    jobText: string = '';

    // Dargestellt wird der leere Name als weiße Fläche
    jobTextClass: string = CSSClass.noJobText;

    // Der Name der Aufzeichnung in der gewünschten Darstellung
    scheduleText: string;

    // Der Verweis zum Ändern der Aufzeichnung
    link: string;

    // Gesetzt, wenn der Auftrag zur Aufzeichnung noch nicht im Archiv liegt
    isActive: boolean;
}

// Bietet das Anlegen einer neuen Aufzeichnung zu einem Auftrag an
class InfoNew implements IInfoRow {
    constructor(legacyId: string, isActive: boolean) {
        this.link = legacyId;
        this.isActive = isActive;
    }

    // Der Name des Auftrags ist immer leer
    jobText: string = '';

    // Dargestellt wird der leere Name als weiße Fläche
    jobTextClass: string = CSSClass.noJobText;

    // Der besondere Name der Aufzeichnung
    scheduleText: string = '(Neue Aufzeichnung)';

    // Der Verweis zum Anlegen einer Aufzeichnung
    link: string;

    // Gesetzt, wenn der Auftrag zur Aufzeichnung noch nicht im Archiv liegt
    isActive: boolean;
}

// Zeigt einen Auftrag als Knoten der Darstellung an
class InfoJob implements IInfoRow {
    constructor(rawInfo: VCRServer.InfoJobContract) {
        this.isActive = rawInfo.active;
        this.jobText = rawInfo.name;
        this.legacyId = rawInfo.id;
    }

    // Der Name des Auftrags
    jobText: string;

    // Die Darstellung des Namens als Knoten im Baum
    jobTextClass: string = CSSClass.jobText;

    // Der Name der Aufzeichnung ist leer
    scheduleText: string = '';

    // Die eindeutige Kennung des Auftrags
    private legacyId: string;

    // Gesetzt, wenn der Auftrag noch nicht im Archiv liegt
    isActive: boolean;

    // Der Auftrag bietet keinen Verweis zur Pflege an
    link: string = '';

    // Aktualisiert die Liste der Aufträge
    static load(whenLoaded: (rows: IInfoRow[]) => void): void {
        VCRServer.getInfoJobs().done((data: VCRServer.InfoJobContract[]) => {
            // Wandeln
            var rows: IInfoRow[] = new Array();

            // Alles auslesen
            $.each(data, (index: number, rawInfo: VCRServer.InfoJobContract) => {
                var job = new InfoJob(rawInfo);

                // Einträge für den Auftrag als Knoten und das Erzeugen einer neuen Aufzeichnung zum Auftrag anlegen
                rows.push(job);
                rows.push(new InfoNew(job.legacyId, job.isActive));

                // Für jede existierende Aufzeichnung einen Eintrag anlegen
                $.map(rawInfo.schedules, (schedule: VCRServer.InfoScheduleContract) => rows.push(new InfoSchedule(schedule, job.isActive)));
            });

            // Aktivieren
            whenLoaded(rows);
        });
    }
}

// Ein einzelner Protokolleintrag
class ProtocolEntry {
    constructor(rawEntry: VCRServer.ProtocolEntryContract) {
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
            case 'EPG': this.displaySource = 'Programmzeitschrift'; break;
            case 'PSI': this.displaySource = 'Sendersuchlauf'; break;
            case 'LIVE': this.displaySource = 'Zapping'; break;
            default: this.displaySource = this.source; break;
        }

        this.files = $.map(rawEntry.files, (file: string) => VCRServer.getFileRoot() + encodeURIComponent(file));
    }

    // Der Startzeitpunkt in der üblichen Kurzanzeige
    displayStart: string = null;

    // Der Endzeitpunkt in der üblichen Kurzanzeige
    displayEnd: string;

    // Das vollständige Enddatum
    fullEnd: string;

    // Die erste Quelle, die angesprochen wurde
    source: string;

    // Die korrigierte Anzeige der Quelle
    displaySource: string;

    // Zeigt Detailinformationen an
    showDetails: (origin: Event) => void;

    // Ein Hinweis zur Größe
    sizeHint: string;

    // Alle Dateien
    files: string[] = new Array();

    // Die primäre Datei
    primaryFile: string = null;
}

////////////////////////// Die einzelnen Seiten //////////////////////////

// Benutzerspezifische Einstellungen
class settingsPage extends Page implements IPage {
    title: string = 'Individuelle Einstellungen ändern';

    visibleLinks: string = '.guideLink, .newLink, .planLink, .currentLink';

    onInitialize(): void {
        VCRServer.UserProfile.global.register(this.registerAsyncCall());
    }

    onShow(): void {
        JMSLib.Bindings.bind(new UserSettingsValidator($('#settingsUpdateButton')), $('#settingsData'));
    }
}

// Fragen und Antworten
class faqPage extends Page implements IPage {
    title: string = '';

    visibleLinks: string = '.guideLink, .newLink, .planLink, .currentLink';

    onInitialize(): void {
        var templateAvailable = this.registerAsyncCall();

        // Name der gewünschten Hilfeseite ermitteln
        var hash = window.location.hash;
        var index = hash.indexOf(';');
        var faq = hash.substr(index + 1);

        // Hilfeseite laden
        JMSLib.TemplateLoader.loadAbsolute('faq/' + faq + '.html').done((template: string) => {
            var content = $(template).find('#faqContents');
            var target = $('#faqContent');

            // Hilfseite einmischen
            target.html(null);
            target.html(content.html());
            target.addClass(CSSClass.faq);

            // Und dann nur noch die Überschrift merken
            this.title = content.attr('data-title');

            templateAvailable();
        });
    }
}

// Die Startseite
class homePage extends Page implements IPage {
    title: string = '';

    visibleLinks: string = '';

    // Die Versionsinformationen des Dienstes
    private serverInfo: VCRServer.InfoServiceContract;

    // Verwaltet die optionale Anzeige
    private detailsManager: JMSLib.DetailManager;

    // Wird zur Aktualisierung aufgerufen
    private startUpdate: () => void;

    // Suchmuster zum ermitteln neuer Versionen.
    private static versionExtract = />VCRNET\.MSI<\/a>[^<]*\s([^\s]+)\s*</i;

    // Aktiviert eine Anzeige zur Aktualisierung
    private showUpdate(button: JQuery, index: number, method: string): void {
        this.startUpdate = () => VCRServer.triggerTask(method).done(() => window.location.hash = '#current');

        // Anzeigen oder ausblenden
        var view = this.detailsManager.toggle(this, button[0], index);
        if (view == null)
            return;

        // Aufbereiten
        view.find('.editButtons').button();
    }

    // Prüft, ob eine neue Version vorliegt.
    private checkUpdate(button: JQuery): void {
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
        xhr.onreadystatechange = () => {
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
    }

    onInitialize(): void {
        var versionAvailable = this.registerAsyncCall();

        // Vorlagen vorbereiten
        this.detailsManager = new JMSLib.DetailManager(1, 'startGuide', 'startScan', 'checkUpdate');

        // Mehr als die Versionsinformationen brauchen wir gar nicht
        VCRServer.getServerVersion().then(data => { this.serverInfo = data; versionAvailable(); });
    }

    onShow(): void {
        var serverInfo = this.serverInfo;

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
            startScan.click(() => this.showUpdate(startScan, 1, 'sourceScan'));

        // Programmzeitschrift kann nur aktualisiert werden, wenn diese nicht über die Konfiguration deaktiviert wurde
        var startGuide = $('#startGuide');
        if (!serverInfo.hasGuides)
            startGuide.removeAttr('href');
        else
            startGuide.click(() => this.showUpdate(startGuide, 0, 'guideUpdate'));

        // Die Versionprüfung kann jeder Anwender aufrufen
        var checkUpdate = $('#checkUpdate');
        checkUpdate.click(() => this.checkUpdate(checkUpdate));

        this.title = 'VCR.NET Recording Service ' + serverInfo.version + ' (' + serverInfo.msiVersion + ')';
    }
}

// Der Aufzeichnungsplan
class planPage extends Page implements IPage {
    title: string = '';

    visibleLinks: string = '.guideLink, .newLink, .refreshLink, .currentLink';

    // Verwaltet die optionale Anzeige
    private detailsManager: JMSLib.DetailManager;

    // Die Vorlage für eine einzelne Aufzeichnung
    private planRowTemplate: JMSLib.HTMLTemplate;

    // Die Vorlage für die Programmzeitschrift
    private guideTemplate: JQuery;

    // Erster zu berücksichtigender Zeitpunkt
    private minStart = new Date(0);

    // Letzter zu berücksichtigender Zeitpunkt
    private maxStart = new Date(0);

    // Die Auswahlliste des Anfangszeitpunktes
    private startChooser: JQuery;

    // Die Option zur Anzeige der Programmzeitschrift
    private ckTasks: JQuery;

    // Aktualisiert die Anzeige
    private refresh(): void {
        // Filter auf den aktuellen Stand bringen
        this.setFilter();

        // Aktualisierung ausführen
        this.planRowTemplate.refresh();
    }

    // Setzt die Grenzwerte für den Filter neu
    private setFilter(): void {
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
        }
        else {
            window.location.hash = '#plan;' + choice;

            // Und Zeitpunkt ermitteln
            this.minStart = new Date(midnight.getTime() + 86400000 * choice);
            this.maxStart = new Date(this.minStart.getTime() + 86400000 * VCRServer.UserProfile.global.planDaysToShow);
        }
    }

    // Führt eine Einschränkung aus
    private filter(item: PlanEntry): boolean {
        if (item.start < this.minStart)
            return false;

        if (item.start >= this.maxStart)
            return false;

        if (item.station == 'EPG')
            return this.ckTasks.is(':checked');

        if (item.station == 'PSI')
            return this.ckTasks.is(':checked');

        return true;
    }

    // Anzeige der Ausnahmeregelung
    private editException(item: PlanEntry, origin: any): void {
        var info = item.exceptionInfo;

        info.startEdit(item, this.detailsManager.toggle(info, origin, 1), () => this.reload(null));
    }

    // Eintrag der Programmzeitschrift abrufen
    private rowGuideEntry(item: PlanEntry, origin: any): void {
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
        item.requestGuide((entry: GuideItem) => {
            // Daten binden
            var html = JMSLib.prepareGuideDisplay(entry, this.guideTemplate, item.start, item.end);

            // Vorbereiten
            html.find('#findInGuide').button();

            // Anzeigen            
            guideElement.replaceWith(html);
            guideReferenceElement.addClass(JMSLib.CSSClass.invisible);
        });
    }

    // Aktuellen Aufzeichnungsplan ermitten
    private reload(whenLoaded: () => void): void {
        // Wir schauen maximal 13 Wochen in die Zukunft
        var endOfTime = new Date(Date.now() + 13 * 7 * 86400000);

        // Zusätzlich beschränken wir uns auf maximal 500 Einträge
        VCRServer.getPlan(500, endOfTime).then(data => {
            // Rohdaten in Modelldaten transformieren
            var plan = $.map(data, (rawData: any) => {
                var item = new PlanEntry(rawData);

                item.onException = (item: PlanEntry, origin: any) => this.editException(item, origin);
                item.onShowGuide = (item: PlanEntry, origin: any) => this.rowGuideEntry(item, origin);
                item.onShowDetails = (item: PlanEntry, origin: any) => {
                    if (this.detailsManager.toggle(item, origin, 0))
                        $(origin.parentNode.parentNode.nextSibling).find('#guideLink').click();
                };

                return item;
            });

            // Details deaktivieren
            this.detailsManager.reset();

            // Liste der Aufzeichnungen aktualisieren
            this.planRowTemplate.loadList(plan);

            // Abschluss melden
            if (whenLoaded != null)
                whenLoaded();
        });
    }

    onShow(): void {
        // Aktuelle Auswahl löschen
        this.startChooser.children().remove();

        // Mitternacht ermitteln
        var midnight = new Date(Date.now());
        var utc = new Date(0);
        utc.setUTCFullYear(midnight.getFullYear());
        utc.setUTCMonth(midnight.getMonth());
        utc.setUTCDate(midnight.getDate());

        // Vorauswahl prüfen
        var hash = window.location.hash;
        var startIndex = hash.indexOf(';');
        var selected = (startIndex >= 0) ? hash.substr(startIndex + 1) : null;
        var rootChecker = (startIndex < 0) ? ' checked="checked"' : '';

        // Neue Auswahl ergänzen
        this.startChooser.append('<input type="radio" id="startChoice0" name="startChoice"' + rootChecker + ' value="0"/><label for="startChoice0">Jetzt</label>');

        // 7 weitere Auswahlpunkte anlegen
        for (var i = 1; i < 7; i++) {
            // Werte berechnen
            var days = i * VCRServer.UserProfile.global.planDaysToShow;
            var daysAsString = days.toString();
            var startDate = new Date(utc.getTime() + 86400000 * days);
            var isSelected = (daysAsString == selected);
            var selector = isSelected ? ' selected="selected"' : '';
            var checker = isSelected ? ' checked="checked"' : '';

            // Neue Auswahl ergänzen            
            this.startChooser.append('<input type="radio" id="startChoice' + i + '" name="startChoice"' + checker + ' value="' + daysAsString + '"/><label for="startChoice' + i + '">' + JMSLib.DateFormatter.getShortDateUtc(startDate) + '</label>');
        }

        // Anzeige aufbereiten
        this.startChooser.buttonset();
        this.startChooser.find('input').click(() => this.refresh());

        // Filter aktivieren
        this.planRowTemplate.filter = (item: PlanEntry) => this.filter(item);

        // Und anzeigen
        this.refresh();

        // Aktualisierung aufsetzen
        $('.refreshLink').click(() => this.reload(null));
        this.ckTasks.click(() => this.refresh());

        this.title = 'Geplante Aufzeichnungen für ' + VCRServer.UserProfile.global.planDaysToShow + ' Tage';
    }

    onInitialize(): void {
        var settingsLoaded = this.registerAsyncCall();
        var guideLoaded = this.registerAsyncCall();
        var planLoaded = this.registerAsyncCall();

        // Elemente suchen
        this.startChooser = $('#startChoice');

        // Darstellung der Auswahl konfigurieren
        this.ckTasks = $('#showTasks');
        this.ckTasks.button();

        // Vorlagen vorbereiten
        this.detailsManager = new JMSLib.DetailManager(2, 'planRowDetails', 'planRowException');
        this.planRowTemplate = JMSLib.HTMLTemplate.dynamicCreate($('#planRows'), 'planRow');

        // Filter deaktivieren
        this.planRowTemplate.filter = (item: PlanEntry) => false;

        // Laden anstossen
        JMSLib.TemplateLoader.load('currentGuide').done((template: string) => {
            this.guideTemplate = $(template).find('#innerTemplate');

            guideLoaded();
        });

        // Benutzerprofile überwachen
        VCRServer.UserProfile.global.register(settingsLoaded);

        // Laden anstossen
        this.reload(planLoaded);
    }
}

// Die Konfiguration und Administration
class adminPage extends Page implements IPage {
    title: string = 'Administration und Konfiguration';

    visibleLinks: string = '.guideLink, .newLink, .planLink, .currentLink';

    // Die Einstellungen zur Sicherheit
    private security: VCRServer.SecuritySettingsContract;

    // Die Einstellungen zu den Aufzeichnungsdateien
    private directory: VCRServer.DirectorySettingsContract;

    // Die Einstellungen der Programmzeitschrift
    private guide: VCRServer.GuideSettingsContract;

    // Die Einstellungen für die Aktualisierung der Quellen
    private scan: VCRServer.SourceScanSettingsContract;

    // Die Konfiguration der Geräte
    private devices: VCRServer.ProfileSettingsContract;

    // Alle anderen Konfigurationsparameter
    private other: VCRServer.OtherSettingsContract;

    // Das aktuell verwendete Regelwerk zur Aufzeichnungsplanung
    private rules: VCRServer.SchedulerRulesContract;

    // Die Liste aller Benutzergruppen
    private static groups: string[] = null;

    // Die Verzeichnisauswahl.
    private directoryBrowser: JQuery;

    // Lädt die Auswahl der Quellen
    private sourceSelections: SourceSelectorLoader;

    // Verwaltung der Quellen 
    private sources: SourceSelector;

    // Vorlage für ein einzelnes Gerät.
    private deviceTemplate: JQuery;

    // Überträgt die Liste der Verzeichnisse in die Oberfläche
    private fillDirectories(directories: string[]): void {
        var selDir = this.directoryBrowser;

        selDir.children().remove();

        $.each(directories, (index: number, directory: string) => {
            if (directory == null)
                selDir.append(new Option('<Bitte auswählen>', ''));
            else
                selDir.append(new Option(directory));
        });
    }

    onInitialize(): void {
        var loadFinished = this.registerAsyncCall();

        this.directoryBrowser = $('#browseDirectory');

        // Gruppen nur einmal laden
        if (adminPage.groups == null) {
            var groupsLoaded = this.registerAsyncCall();

            VCRServer.getWindowsGroups().done((data: string[]) => { adminPage.groups = data; groupsLoaded(); });
        }

        // Alles nacheinander laden - die Zahl der gleichzeitig offenen Requests ist beschränkt!
        VCRServer
            .browseDirectories('', true)
            .then<any>((directories: string[]) => { this.fillDirectories(directories); return VCRServer.getSecuritySettings(); })
            .then<any>((data: VCRServer.SecuritySettingsContract) => { this.security = data; return VCRServer.getDirectorySettings(); })
            .then<any>((data: VCRServer.DirectorySettingsContract) => { this.directory = data; return VCRServer.getGuideSettings(); })
            .then<any>((data: VCRServer.GuideSettingsContract) => { this.guide = data; return VCRServer.getSourceScanSettings(); })
            .then<any>((data: VCRServer.SourceScanSettingsContract) => { this.scan = data; return VCRServer.getProfileSettings(); })
            .then<any>((data: VCRServer.ProfileSettingsContract) => { this.devices = data; return JMSLib.TemplateLoader.load('adminDevices'); })
            .then<any>((template: string) => { $('#devices').append($(template).find('#template').children()); return VCRServer.getOtherSettings(); })
            .then<any>((data: VCRServer.OtherSettingsContract) => { this.other = data; return VCRServer.getSchedulerRules(); })
            .then<any>((data: VCRServer.SchedulerRulesContract) => { this.rules = data; return JMSLib.TemplateLoader.load('adminRules'); })
            .then<any>((template: string) => { $('#rules').append($(template).find('#template').children()); loadFinished(); });

        // Geräte laden
        var profiles = $('#profileForGuide');
        var sourcesLoaded = this.registerAsyncCall();

        this.loadProfiles(profiles);
        this.sourceSelections = new SourceSelectorLoader(profiles);
        this.sourceSelections.loadTemplates(() => {
            this.sources = this.sourceSelections.appendAfter($('#guideSourceSelector').find('tbody').children().last(), true);

            sourcesLoaded();
        });

        // Vorlagen laden
        var templateLoaded = this.registerAsyncCall();
        JMSLib.TemplateLoader.load('profileRow').done((template: string) => {
            this.deviceTemplate = $(template).find('#template');

            templateLoaded();
        });

        // Uhrzeiten laden
        JMSLib.HourListSettings.createHourButtons($('.guideHours'), 'guideHour');
        JMSLib.HourListSettings.createHourButtons($('.scanHours'), 'scanHour');

        // Und schließlich brauchen wir noch die Konfiguration des Anwenders
        VCRServer.UserProfile.global.register(this.registerAsyncCall());
    }

    // Schaltet alle Oberflächen Elemnte ab und zeigt vor dem verzögerten Übergang auf die Startseite eine knappe Information an.
    private static restartServer(): void {
        $('.adminView').addClass(JMSLib.CSSClass.invisible);
        $('.linkArea').addClass(JMSLib.CSSClass.invisible);
        $('.serverRestart').removeClass(JMSLib.CSSClass.invisible);

        window.setTimeout(() => window.location.reload(), 10000);
    }

    // Aktualisiert Konfigurationsdaten und kehrt dann im Allgemeinen auf die Startseite zurück.
    private update(type: string, contract: VCRServer.SettingsContract, button: JQuery, filter: (key: string, value: any) => any = null): void {
        button.removeAttr('title');

        VCRServer
            .updateConfiguration(type, contract, filter)
            .done((data?: boolean) => {
                // Ergebnis bearbeiten
                if (data == null)
                    button.addClass(CSSClass.danger);
                else if (data)
                    adminPage.restartServer();
                else {
                    VCRServer.RecordingDirectoryCache.reset();

                    window.location.hash = '#home';
                }
            })
            .fail(JMSLib.dispatchErrorMessage((message: string) => {
                // Fehler bearbeiten
                button.attr('title', message);
                button.addClass(CSSClass.danger);
            }));
    }

    // Ergänzt ein Verzeichnis.
    private addDirectory(): void {
        var shareInput = $('#networkShare');
        var list = $('#recordingDirectories');

        var share: string = shareInput.val();
        if (share != null)
            if (share.length > 0) {
                VCRServer.validateDirectory(share).done((ok: boolean) => {
                    JMSLib.Bindings.setErrorIndicator(shareInput, ok ? null : 'Ungültiges Verzeichnis.');

                    if (!ok)
                        return;

                    shareInput.val(null);
                    list.append(new Option(share));
                });

                return;
            }

        var selected: string = $('#browseDirectory').val();
        if (selected == null)
            return;
        if (selected.length < 1)
            return;

        list.append(new Option(selected));
    }

    // Bereitet die Anzeige der Sicherheitseinstellungen vor
    private showSecurity(): void {
        // Auswahllisten für Benutzer
        var selUser = $('#selUserGroup');
        var selAdmin = $('#selAdminGroup');

        // Benutzergruppen laden
        $.each(adminPage.groups, (index: number, group: string) => {
            selUser.append(new Option(group));
            selAdmin.append(new Option(group));
        });

        // Speichern vorbereiten
        var securityUpdate = $('#updateSecurity');
        securityUpdate.click(() => this.update('security', this.security, securityUpdate));
    }

    // Bereitet die Anzeige der Verzeichnisse vor
    private showDirectory(): void {
        // Oberflächenelemente
        var directoryUpdate = $('#updateDirectory');
        var recording = $('#recordingDirectories');
        var discard = $('#removeDirectory');
        var toParent = $('#toParentDir');
        var accept = $('#acceptDir');

        // Aktuelle Auswahl
        $.each(this.directory.directories, (index: number, directory: string) => recording.append(new Option(directory)));

        discard.click(() => recording.find(':checked').remove());
        accept.click(() => this.addDirectory());

        // Navigation
        toParent.click(() => {
            var root = this.directoryBrowser.children().first().val();

            VCRServer
                .browseDirectories(root, false)
                .done((directories: string[]) => this.fillDirectories(directories));
        });
        this.directoryBrowser.change(() => {
            var dir: string = this.directoryBrowser.val();
            if (dir == null)
                return;
            if (dir.length < 1)
                return;

            VCRServer
                .browseDirectories(dir, true)
                .done((directories: string[]) => this.fillDirectories(directories));
        });

        // Speichern
        directoryUpdate.click(() => {
            this.directory.directories = $.map($('#recordingDirectories option'), (option: any): string => option.value);

            this.update('directory', this.directory, directoryUpdate);
        });
    }

    // Bereitet die Konfiguration der Programmzeitschrift vor
    private showGuide(): void {
        // Auswahl neuer Quellen vorbereiten
        this.sources.initialize();

        // Oberflächenelemente
        var guideUpdate = $('#updateGuide');
        var discard = $('#removeFromGuide');
        var selSource = $('#guideSources');
        var selector: any = selSource[0];
        var accept = $('#addToGuide');

        // Aktuelle Auswahl
        $.each(this.guide.sources, (index: number, source: string) => selSource.append(new Option(source)));

        discard.click(() => selSource.find(':checked').remove());
        accept.click(() => {
            var source = this.guide['sourceName'];
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

            this.sources.reset();
        });

        // Speichern
        guideUpdate.click(() => {
            if (GuideSettingsValidator.activated(this.guide))
                this.guide.sources = $.map($('#guideSources option'), (option: any): string => option.value);
            else
                this.guide.duration = 0;

            this.update('guide', this.guide, guideUpdate, GuideSettingsValidator.filterPropertiesOnSend);
        });
    }

    // Bereitet die Konfiguration für die Aktualisierung der Quellen vor
    private showScan(): void {
        // Oberflächenelemente
        var scanUpdate = $('#updateScan');

        // Speichern
        scanUpdate.click(() => {
            switch (SourceScanSettingsValidator.mode(this.scan)) {
                case 'D': this.scan.interval = 0; break;
                case 'M': this.scan.interval = -1; break;
                case 'P': break;
            }

            this.update('scan', this.scan, scanUpdate, SourceScanSettingsValidator.filterPropertiesOnSend);
        });
    }

    // Bereitet die Konfiguration für die Nutzung der Geräteprofile vor
    private showDevices(): void {
        // Oberflächenelemente
        var deviceUpdate = $('#updateDevices');
        var devices = $('#selDefaultProfile');

        $.each(this.devices.profiles, (index: number, profile: VCRServer.ProfileContract) => devices.append(new Option(profile.name)));

        // Speichern
        deviceUpdate.click(() => this.update('devices', this.devices, deviceUpdate));
    }

    // Bereitet die Konfiguration sonstiger Einstellungen vor
    private showOther(): void {
        // Oberflächenelemente
        var otherUpdate = $('#updateOther');

        // Speichern
        otherUpdate.click(() => {
            var settings = this.other;

            switch (OtherSettingsValidator.hibernationMode(settings)) {
                case 'OFF': settings.mayHibernate = false; break;
                case 'S3': settings.mayHibernate = true; settings.useStandBy = true; break;
                case 'S4': settings.mayHibernate = true; settings.useStandBy = false; break;
            }

            this.update('other', settings, otherUpdate);
        });
    }

    // Bereitet die Eingabe der Planungsregeln vor
    private showRules(): void {
        // Oberflächenelemente
        var rulesUpdate = $('#updateRules');

        // Speichern
        rulesUpdate.click(() => this.update('rules', this.rules, rulesUpdate));
    }

    onShow(): void {
        var navigator = $('#adminTabs');
        var options: JQueryUI.TabsOptions = {};

        // Eventuell sollen wir in einer bestimmten Situation starten
        var hash = window.location.hash;
        var currentTabIndex = hash.indexOf(';');
        if (currentTabIndex >= 0)
            switch (hash.substr(currentTabIndex + 1)) {
                case 'directories': options.active = 1; break;
                case 'security': options.active = 0; break;
                case 'devices': options.active = 2; break;
                case 'sources': options.active = 4; break;
                case 'guide': options.active = 3; break;
                case 'rules': options.active = 5; break;
                case 'other': options.active = 6; break;
            }

        // Oberfläche vorbereiten
        navigator.tabs(options).addClass('ui-tabs-vertical ui-helper-clearfix');
        navigator.on('tabsactivate', function (ev: JQueryEventObject) {
            window.location.hash = '#admin;' + arguments[1].newPanel[0].id;
        });

        // Oberfläche vorbereiten
        $('.editButtons').button();

        // Alle Bereiche vorbereiten
        this.showDirectory();
        this.showSecurity();
        this.showDevices();
        this.showOther();
        this.showGuide();
        this.showRules();
        this.showScan();

        // Besondere Teilaspekte
        var profileValidator = new ProfileSettingsValidator(this.devices, $('#updateDevices'));

        // An die Oberfläche binden
        JMSLib.Bindings.bind(new DirectorySettingsValidator(this.directory, $('#updateDirectory')), $('#directories'));
        JMSLib.Bindings.bind(new GuideSettingsValidator(this.guide, $('#updateGuide'), $('#guideForm')), $('#guide'));
        JMSLib.Bindings.bind(new SourceScanSettingsValidator(this.scan, $('#updateScan')), $('#sources'));
        JMSLib.Bindings.bind(new OtherSettingsValidator(this.other, $('#updateOther')), $('#other'));
        JMSLib.Bindings.bind(new SecuritySettingsValidator(this.security), $('#security'));
        JMSLib.Bindings.bind(new ScheduleRulesValidator(this.rules), $('#rules'));
        JMSLib.Bindings.bind(profileValidator, $('#devices'));

        // Besondere Teilaspekte
        profileValidator.bindProfiles($('#deviceList'), this.deviceTemplate);

        // Oberfläche vorbereiten
        $('.' + JMSLib.CSSClass.hourSetting).button();
    }
}

// Alle Aktivitäten
class currentPage extends Page implements IPage {
    title: string = 'Geräteübersicht';

    visibleLinks: string = '.refreshLink, .guideLink, .planLink, .newLink';

    // Verwaltung der Detailansichten
    private detailsManager: JMSLib.DetailManager;

    // Die Tabelle mit den einzelnen Aktivitäten
    private table: JMSLib.HTMLTemplate;

    // Führt eine vollständige Aktualisierung aus
    private reload(): void {
        CurrentInfo.load((infos: CurrentInfo[]) => {
            this.detailsManager.reset();
            this.table.loadList(infos);
        })
    }

    // Anzeige der Programmzeitschrift einer einzelnen Aktivität
    private showGuide(item: CurrentInfo, origin: any): void {
        var detailsManager = this.detailsManager;

        item.requestGuide((entry: GuideItem) => {
            // Anzeige vorbereiten
            var view = detailsManager.toggle(entry, origin, 0, (guideItem: GuideItem, template: JQuery) => JMSLib.prepareGuideDisplay(guideItem, template, item.start, item.end));
            if (view != null)
                view.find('#findInGuide').button();
        });
    }

    // Eine laufende Aufzeichnung manipulieren
    private startAbort(item: CurrentInfo, origin: any): void {
        // Auswahlwerte zurücksetzen
        item.suppressHibernate = VCRServer.UserProfile.global.noHibernateOnAbort;
        item.remainingMinutes = item.originalRemainingMinutes;

        var template = this.detailsManager.toggle(item, origin, 1);
        if (template == null)
            return;

        // Aktualisierung
        var refresh = () => JMSLib.HTMLTemplate.applyTemplate(item, template);

        // Konfiguration der Einstellungen
        var options =
            {
                slide: (slider: any, newValue: any) => { item.remainingMinutes = newValue.value; refresh(); },
                value: item.remainingMinutes,
                max: 480,
                min: 0,
            };

        var slider = template.find('#slider');
        var sendButton = template.find('#sendButton');
        var disableButton = template.find('#disableButton');

        // Oberfläche aufbereiten
        slider.slider(options);
        disableButton.button()
        sendButton.button();

        // Aktionen definiert
        sendButton.click(() => { sendButton.button('disable', true); item.updateEndTime(() => this.reload()); });
        disableButton.click(() => { slider.slider('value', item.remainingMinutes = 0); refresh(); });
    }

    onShow(): void {
        $('.refreshLink').click(() => this.reload());
    }

    onInitialize(): void {
        var profileAvailable = this.registerAsyncCall();
        var tableAvailable = this.registerAsyncCall();

        // Vorlagen laden
        this.detailsManager = new JMSLib.DetailManager(2, 'currentGuide', 'editCurrent');
        this.table = JMSLib.HTMLTemplate.dynamicCreate($('#currentTable'), 'currentRow');

        // Ereignisse anmelden
        CurrentInfo.guideDisplay = (item: CurrentInfo, origin: any) => this.showGuide(item, origin);
        CurrentInfo.startAbort = (item: CurrentInfo, origin: any) => this.startAbort(item, origin);

        // Wir brauchen auch die Benutzereinstellungen
        VCRServer.UserProfile.global.register(profileAvailable);

        // Und natürlich die eigentlichen Daten
        CurrentInfo.load((infos: CurrentInfo[]) => { this.table.loadList(infos); tableAvailable(); });
    }
}

// Aufzeichung anlegen, bearbeiten und löschen
class editPage extends Page implements IPage {
    title: string = '';

    visibleLinks: string = '.guideLink, .planLink, .currentLink';

    // Die Auswahl der Quellen
    private sourceSelections: SourceSelectorLoader;

    // Die Pflege der Ausnahmen
    private exceptionRowTemplate: JMSLib.HTMLTemplate;

    // Verwaltung der Quellen für den Auftrag
    private jobSources: SourceSelector;

    // Verwaltung der Quellen für die Aufzeichnung
    private scheduleSources: SourceSelector;

    // Vorgeladende Daten
    private existingData: VCRServer.JobScheduleInfoContract;

    // Gesetzt, wenn der Aufruf aus der Programmzeitschrift heraus erfolgte
    private fromGuide: boolean;

    onShow(): void {
        // Modelldaten erstellen
        var job = new JobDataValidator(this.existingData, $('#update'), this.sourceSelections.profileSelector.val());
        var schedule = new ScheduleDataValidator(job, this.existingData);

        // Rücksprung vorbereiten.
        if (this.fromGuide)
            job.model.guideAfterAdd = VCRServer.UserProfile.global.guideAfterAdd;

        // Datenübertragung vorbereiten
        JMSLib.Bindings.bind(job, $('#jobData'));
        JMSLib.Bindings.bind(schedule, $('#scheduleData'));

        // Quellenauswahl vorbereiten
        this.jobSources.initialize();
        this.scheduleSources.initialize();

        // Überschrift setzen
        if (job.model.id == null)
            this.title = 'Neuen Auftrag anlegen';
        else if (schedule.model.id == null)
            this.title = 'Neue Aufzeichnung anlegen';
        else {
            this.title = 'Aufzeichnung bearbeiten';

            var deleteButton = $('#delete');
            deleteButton.button();
            deleteButton.click((scope: JQueryEventObject) => job.prepareDelete(scope));
            deleteButton.removeClass(JMSLib.CSSClass.invisible);
        }

        // Ausnahmen übernehmen
        var exceptions = schedule.model.exceptionInfos;
        if (exceptions.length > 0)
            this.exceptionRowTemplate.loadList(exceptions);
        else
            $('#exceptionArea').addClass(JMSLib.CSSClass.invisible);
    }

    onInitialize(): void {
        var templatesLoaded = this.registerAsyncCall();
        var directoriesLoaded = this.registerAsyncCall();

        // Die Informationen, die wir gerade verändern
        this.sourceSelections = new SourceSelectorLoader($('#selProfile'));
        this.exceptionRowTemplate = JMSLib.HTMLTemplate.dynamicCreate($('#exceptionRows'), 'exceptionRow');

        // Auslesen der Kennung - für FireFox ist es nicht möglich, .hash direkt zu verwenden, da hierbei eine Decodierung durchgeführt wird
        var query = window.location.href.split("#")[1];
        var idIndex = query.indexOf(';id=');
        var hasId = (idIndex >= 0);
        var jobScheduleId: string = hasId ? query.substr(idIndex + 4) : '';
        var epgIndex = jobScheduleId.indexOf(';epgid=');
        var epgId: string = '';

        if (this.fromGuide = (epgIndex >= 0)) {
            epgId = jobScheduleId.substr(epgIndex + 7);
            jobScheduleId = jobScheduleId.substr(0, epgIndex);
        }

        // Konfiguration der Datumsauswahl
        var germanMonths: string[] = ['Jan', 'Feb', 'Mrz', 'Apr', 'Mai', 'Jun', 'Jul', 'Aug', 'Sep', 'Okt', 'Nov', 'Dez'];
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
            firstDay: 1,
        };

        // Datumsauswahl vorbereiten
        startDate.datepicker(dateOptions);
        endDate.datepicker(dateOptions);

        // Daten zur Aufzeichnung laden
        if (hasId) {
            var epgLoaded = this.registerAsyncCall();

            VCRServer.createScheduleFromGuide(jobScheduleId, epgId).then(data => { this.existingData = data; epgLoaded(); });
        }

        // Liste der Geräteprofile laden
        this.loadProfiles($('#selProfile'));

        // Liste der Verzeichnisse wählen
        VCRServer.RecordingDirectoryCache.getPromise().then(directories => {
            var directoryList = $('#selDirectory');

            $.each(directories, (index: number, directory: string) => directoryList.append(new Option(directory)));

            directoriesLoaded();
        });

        // Vorlage für die Auswahl der Quellen laden
        this.sourceSelections.loadTemplates(() => {
            // In die Oberfläche einbinden
            this.jobSources = this.sourceSelections.appendAfter($('#jobData').find('tbody').children().last());
            this.scheduleSources = this.sourceSelections.appendAfter($('#scheduleData').find('tbody').children().first());

            templatesLoaded();
        });

        // Besondere Aktionen sobald die Benutzerkonfiguration verändert wurde
        VCRServer.UserProfile.global.register(this.registerAsyncCall());
    }
}

// Die Programmzeitschrift
class guidePage extends Page implements IPage {
    title: string = 'Programmzeitschrift';

    visibleLinks: string = '.newLink, .planLink, .currentLink, .favLink';

    // Alle verwendeten Geräteprofile
    private profiles: VCRServer.ProfileInfoContract[];

    // Die Vorlage für die Tabelle
    private guideTable: JMSLib.HTMLTemplate;

    // Die Verwaltung der Detailansicht
    private details: JMSLib.DetailManager;

    // Die Vorlage für die Sendungsinformationen in der Detailansicht
    private guideTemplate: JQuery;

    // Alle Aufträge des aktuell gewählten Geräteprofils
    private jobs: VCRServer.ProfileJobInfoContract[];

    // Die gespeicherte Suche, die angezeigt werden soll
    private startWith: SavedGuideQuery;

    // Klappt die Detailansicht auf oder zu
    private showDetails(guideItem: GuideItem, origin: any): void {
        var view = this.details.toggle(guideItem, origin, 0);
        if (view == null)
            return;

        // Referenzelement in der Anzeige ermitteln
        var guideReferenceElement = $(origin.parentNode.parentNode);
        var guideElement = view.find('#planRowProgramGuide');

        // Anzeigen
        var details = JMSLib.HTMLTemplate.cloneAndApplyTemplate(guideItem, this.guideTemplate);
        details.find('#guideHeadline').addClass(JMSLib.CSSClass.invisible);
        details.find('#findInGuide').button();

        // Auswahlliste mit den Aufträgen füllen
        var jobSelector = view.find('#selJob');
        $.each(this.jobs, (index: number, job: VCRServer.ProfileJobInfoContract) => jobSelector.append(new Option(job.name, '*' + job.id)));

        // Fertig aufbereitetes Element einsetzen
        guideElement.replaceWith(details);

        // Schaltfläche zum Anlegen einer neuen Aufzeichnung aktivieren
        var createCommand = view.find('#createNew');
        createCommand.button();

        // Bei jeder Änderung der Auswahl des Auftrags wird das Ziel geeignet angepasst und so das normale Verhalten eines Verweises erreicht
        function jobChanged(): void {
            createCommand.attr('href', '#edit;id=' + jobSelector.val() + ';epgid=' + encodeURIComponent(guideItem.id));
        }

        // Wird ein Auftrag ausgewählt muss der Verweis gesetzt werden
        jobSelector.change(jobChanged);

        // Auf jeden Fall aber einmalig vor der Anzeige
        jobChanged();
    }

    // Aktualisiert die Liste der Sendungen
    private refresh(): void {
        $('#addFavorite').button({ 'disabled': GuideFilter.global.title == null });

        // Einschränkung auf die Quellesuche machen wir nur, wenn keine Quelle ausgewählt ist
        var sourceFilter = $('.guideSourceFilter');
        if ((GuideFilter.global.station || '') == '')
            sourceFilter.removeClass(JMSLib.CSSClass.hide);
        else
            sourceFilter.addClass(JMSLib.CSSClass.hide);

        GuideFilter.global.execute((items: GuideItem[]) => {
            $.each(items, (index: number, item: GuideItem) => item.onShowDetails = (target: GuideItem, origin: any) => this.showDetails(target, origin));

            this.details.reset();
            this.guideTable.loadList(items);

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
    }

    // Wird aufgerufen, wenn der Anwender das Gerät verändert
    private deviceChanged(filterIsCurrent: boolean): void {
        var newDevice = $('#selDevice').val();

        // Liste leeren
        this.jobs = new Array();

        // Aufträge zum Gerät anfordern
        VCRServer.getProfileJobInfos(newDevice).then(jobs => {
            this.jobs = jobs;

            // Infomationen zum Gerät anfordern
            VCRServer.GuideInfoCache.getPromise(newDevice).then(data => {
                // Eventuell gibt es das Gerät schon nicht mehr
                if (data == null)
                    return;

                // Die aktuelle Quelle im Filter
                var station = GuideFilter.global.station;
                var info = new VCRServer.GuideInfo(data);

                // Liste der Quellen ganz neu füllen
                var selSource = $('#selSource');
                selSource.children().remove();
                selSource.append(new Option('(Alle Sender)', ''));
                $.each(info.stations, (index: number, station: string) => selSource.append(new Option(station)));

                // Dann versuchen wir, den ursprünglich gewählten Wert zu reaktivieren
                selSource.val(station);

                // Liste der Tage ganz neu füllen
                var selDay = $('#selectDay');
                selDay.children().remove();
                selDay.append('<a href="javascript:void(0)" data-day="0">Jetzt</a>');
                var firstDay = info.firstStart;
                if (firstDay != null) {
                    firstDay = new Date(firstDay.getFullYear(), firstDay.getMonth(), firstDay.getDate());

                    // Maximal 14 Tage, sonst wird die Liste zu unübersichtlich
                    for (var n = 14; (n-- > 0) && (firstDay < info.lastStart);) {
                        selDay.append('<a href="javascript:void(0)" data-day="' + firstDay.toISOString() + '">' + JMSLib.DateFormatter.getShortDate(firstDay) + '</a>');

                        firstDay = new Date(firstDay.getTime() + 86400000);
                    }
                }

                // Änderungen an der Auswahl der Tage ab nun überwachen
                var daySelectors = selDay.find('a');
                daySelectors.button();
                daySelectors.click((ev: JQueryEventObject) => this.dayChanged(ev));

                // Nun können wir eine Aktualisierung anfordern
                if (!filterIsCurrent)
                    GuideFilter.global.changeDevice(newDevice, selSource.val());
            });
        });
    }

    // Der Anwender hat eine Uhrzeit ausgewählt, ab der die Anzeige erfolgen soll
    private hourChanged(eventObject: JQueryEventObject): void {
        // Was hat der Anwender gewählt?
        var target: any = eventObject.currentTarget;
        var hour = parseInt(target.getAttribute('data-hour')) * 3600000;

        // Den aktuellen Tag ermitteln
        var start = GuideFilter.global.start;
        if (start == null)
            start = new Date($.now()).toISOString();

        // Tag und Uhrzeit kombinieren und aktualisieren
        var fullStart = new Date(start);
        var dayStart = new Date(fullStart.getFullYear(), fullStart.getMonth(), fullStart.getDate());

        GuideFilter.global.changeStart(new Date(dayStart.getTime() + hour).toISOString());
    }

    // Der Anwender hat einen Tag ausgewählt, ab dem die Anzeige erfolgen soll
    private dayChanged(eventObject: JQueryEventObject): void {
        // Was hat der Anwender gewählt?
        var target: any = eventObject.currentTarget;

        // Tag ermitteln und anzeigen
        var newStart = target.getAttribute('data-day');
        if (newStart === '0')
            newStart = null;

        GuideFilter.global.changeStart(newStart);
    }

    onShow(): void {
        $('.firstButton, .prevButton, .nextButton, .guideHours a, .scanHours a, #resetAll').button();

        // Zum Hinzufügen von gespeicherten Suchen
        var add = $('#addFavorite');
        add.button({ disabled: GuideFilter.global.title == null });
        add.click(() => {
            if (GuideFilter.global.title == null)
                return;

            var query = new SavedGuideQuery();
            query.titleOnly = GuideFilter.global.content == null;
            query.source = GuideFilter.global.station;
            query.device = GuideFilter.global.device;
            query.text = GuideFilter.global.title;

            if ((query.source || '') == '') {
                query.encryption = GuideFilter.global.cryptFilter;
                query.sourceType = GuideFilter.global.typeFilter;
            }
            else {
                query.encryption = VCRServer.GuideEncryption.ALL;
                query.sourceType = VCRServer.GuideSource.ALL;
            }

            SavedGuideQueries.load().splice(0, 0, query);
            SavedGuideQueries.save();

            add.button({ disabled: true });
        });

        // Wenn wir Geräteprofile haben, müssen wir etwas mehr tun
        if (this.profiles.length > 0) {
            // Die Auswahlliste der Geräte füllen
            var selDevice = $('#selDevice');
            $.each(this.profiles, (index: number, profile: VCRServer.ProfileInfoContract) => selDevice.append(new Option(profile.name)));

            // Vorauswahl aus dem Filter übernehmen
            var device = GuideFilter.global.device;
            if (device != null)
                selDevice.val(device);

            // Von nun an auf Änderungen überwachen
            selDevice.change(() => this.deviceChanged(false));

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
            var textChanged = (): void => { GuideFilter.global.changeQuery(searchText.val(), checkContent.prop('checked')); }
            searchText.on('change', textChanged);
            searchText.on('input', textChanged);
            checkContent.change(textChanged);

            // Sonstige Änderungen überwachen
            var selSource = $('#selSource');
            selSource.change((ev: JQueryEventObject) => GuideFilter.global.changeStation(selSource.val()));
            $('.guideHours a').click((ev: JQueryEventObject) => this.hourChanged(ev));
            $('.firstButton').click(() => GuideFilter.global.firstPage());
            $('.prevButton').click(() => GuideFilter.global.prevPage());
            $('.nextButton').click(() => GuideFilter.global.nextPage());
            $('#resetAll').click(() => {
                var toBeChecked = $('#withContent, #guideTVAndRadio, #guideFreeAndPay');
                toBeChecked.prop('checked', true);
                toBeChecked.button('refresh');
                searchText.val(null);
                selSource.val(null);

                $('.guideSourceFilter').buttonset('refresh');

                GuideFilter.global.reset();
            });

            // Von nun an auf den Filter reagieren
            GuideFilter.global.onChange = () => this.refresh();

            // Erste Aktualisierung
            this.deviceChanged(false);
        }

        // Schauen wir mal, ob wir eine gespeicherte Suche anzeigen müssen
        var startWith = this.startWith;
        if (startWith != null) {
            $('#withContent').prop('checked', !startWith.titleOnly);
            $('#searchText').val(startWith.text.substr(1));
            $('#selDevice').val(startWith.device);
            $('#selSource').val(startWith.source);
            $('#withContent').button('refresh');

            var selectedType = $('input[name="guideSourceType"][value="' + startWith.sourceType + '"]');
            selectedType.prop('checked', true);
            selectedType.button('refresh');

            var encryption = $('input[name="guideSourceCrypt"][value="' + startWith.encryption + '"]');
            encryption.prop('checked', true);
            encryption.button('refresh');

            $('.guideSourceFilter').buttonset('refresh');

            GuideFilter.global.content = startWith.titleOnly ? null : startWith.text;
            GuideFilter.global.cryptFilter = startWith.encryption;
            GuideFilter.global.typeFilter = startWith.sourceType;
            GuideFilter.global.station = startWith.source;
            GuideFilter.global.device = startWith.device;
            GuideFilter.global.title = startWith.text;

            GuideFilter.global.changeStart(null);

            add.button({ disabled: true });
        }
    }

    onInitialize() {
        // Schauen wir mal, ob wir einen Favoriten anzeigen sollen
        this.startWith = SavedGuideQuery.guideScope;
        SavedGuideQuery.guideScope = null;

        var profilesLoaded = this.registerAsyncCall();
        var settingsLoaded = this.registerAsyncCall();
        var guideLoaded = this.registerAsyncCall();

        // Vorlagen verbinden
        this.guideTable = JMSLib.HTMLTemplate.dynamicCreate($('#guideTable'), 'guideRow');
        this.details = new JMSLib.DetailManager(2, 'guideDetails');

        // Liste der Geräteprofile laden
        VCRServer.ProfileCache.getPromise().then(data => {
            this.profiles = data;

            profilesLoaded();
        });

        // Zusätzliche Vorlagen laden
        JMSLib.TemplateLoader.load('currentGuide').done((template: string) => {
            this.guideTemplate = $(template).find('#innerTemplate');

            guideLoaded();
        });

        // Benutzereinstellungen abwarten
        VCRServer.UserProfile.global.register(() => { GuideFilter.global.userProfileChanged(); settingsLoaded(); });

        // Quellauswahl aufbereiten
        var filter = $('.guideSourceFilter');
        filter.buttonset();

        var sourceTypeFilter = $('#guideSourceType');
        sourceTypeFilter.change(() => GuideFilter.global.changeType(sourceTypeFilter.find(':checked').val()));
        var encryptionFilter = $('#guideSourceCrypt');
        encryptionFilter.change(() => GuideFilter.global.changeEncryption(encryptionFilter.find(':checked').val()));
    }
}

// Die Liste aller Aufträge
class jobsPage extends Page implements IPage {
    title: string = 'Alle Aufträge';

    visibleLinks: string = '.guideLink, .newLink, .planLink, .currentLink';

    // Die Verwaltung der aktuellen Darstellung
    private table: JMSLib.HTMLTemplate;

    // Das Oberflächenelement zur Umschaltung der Darstellung
    private isActive: JQuery;

    onInitialize() {
        var loaded = this.registerAsyncCall();

        // Aus der Vorlage erzeugen
        this.isActive = $('#selActive');
        this.table = JMSLib.HTMLTemplate.dynamicCreate($('#jobTable'), 'jobRow');
        this.table.filter = (row: IInfoRow) => row.isActive == this.isActive.is(':checked');

        // Voreingestellte Auswahl setzen - das müssen wir machen, bevor wir die Darstellung umstellen
        var showArchive = (window.location.hash.indexOf(';archive') >= 0);
        if (showArchive)
            $('#selArchive').prop('checked', true);
        else
            this.isActive.prop('checked', true);

        // Auswahl aufbereiten
        var filter = $('#filter');
        filter.buttonset();
        filter.change(() => this.table.refresh());

        // Ladevorgang ausführen
        InfoJob.load((rows: IInfoRow[]) => { this.table.loadList(rows); loaded(); });
    }
}

// Die Liste der Protokolle
class logPage extends Page implements IPage {
    title: string = 'Aufzeichnungsprotokolle einsehen';

    visibleLinks: string = '.guideLink, .newLink, .planLink, .currentLink';

    // Das Referenzdatum für die Auswahllisten
    private referenceDay: Date;

    // Die Vorlage für einen einzelne Eintrag
    private rowTemplate: JMSLib.HTMLTemplate;

    // Verwaltet die optionale Anzeige der Details
    private detailsManager: JMSLib.DetailManager;

    // Prüft einen Eintrag
    private filter(entry: ProtocolEntry): boolean {
        if (entry.source == 'EPG')
            return $('#withGuide').is(':checked');
        if (entry.source == 'PSI')
            return $('#withScan').is(':checked');
        if (entry.source == 'LIVE')
            return $('#withLive').is(':checked');

        return true;
    }

    // Aktualisiert lediglich die Filter auf der aktuellen Menge von Protokolleinträgen
    private refresh(): void {
        this.detailsManager.reset();
        this.rowTemplate.refresh();
    }

    // Fordert neue Protokolleinträge an
    private reload(whenLoaded: () => void = null): void {
        var profile = $('#selProfile').val();
        var endDay = new Date($('#selDate').val());
        var startDay = new Date(endDay.getTime() - 7 * 86400000);

        VCRServer.getProtocolEntries(profile, startDay, endDay).done((entries: VCRServer.ProtocolEntryContract[]) => {
            // Wir zeigen den neuesten oben 
            entries.reverse();

            var models = $.map(entries, (entry: VCRServer.ProtocolEntryContract) => {
                var model = new ProtocolEntry(entry);

                // Detailanzeige vorbereiten
                model.showDetails = (origin: Event) => {
                    // Das war ein Ausblenden der Detailanzeige
                    var view = this.detailsManager.toggle(model, origin.currentTarget, 0);
                    if (view == null)
                        return;

                    // Mal schauen, ob es noch Aufzeichnungsdateien gibt
                    var fileUI = view.find('#files');
                    var files = model.files;

                    // Aufzeichnungsdatein zur Anzeige anbieten
                    if (files.length < 1)
                        fileUI.parent().remove();
                    else
                        $.each(files, (index: number, url: string) => fileUI.append('<a href="' + url + '"><img src="ui/images/recording.png" class="linkImage"/></img>'));

                    // Es gibt keine Primärdatei, deren Namen wir anzeigen könnten
                    if ((model.primaryFile == null) || (model.primaryFile.length < 1))
                        $('#primaryFile').parent().remove();
                };

                return model;
            });

            // Neu laden
            this.detailsManager.reset();
            this.rowTemplate.loadList(models);

            // Fertig - beim Starten wartet jemand auf uns
            if (whenLoaded != null)
                whenLoaded();
        });
    }

    onInitialize(): void {
        var profilesLoaded = this.registerAsyncCall();
        var rawNow = new Date($.now());
        var dayList = $('#selDate');

        // Referenz setzen
        this.referenceDay = new Date(Date.UTC(rawNow.getFullYear(), rawNow.getMonth(), rawNow.getDate()));

        // Auswahlliste füllen
        for (var i = 0; i < 10; i++) {
            var curDay = new Date(this.referenceDay.getTime() - 7 * i * 86400000);
            var curMonth = 1 + curDay.getUTCMonth();
            var curDate = curDay.getUTCDate();

            dayList.append(new Option(JMSLib.DateFormatter.formatNumber(curDate) + '.' + JMSLib.DateFormatter.formatNumber(curMonth), curDay.toISOString()));
        }

        // Auf Änderungen reagieren
        dayList.change(() => this.reload());

        // Vorlagen vorbereiten
        this.rowTemplate = JMSLib.HTMLTemplate.dynamicCreate($('#logRows'), 'logRow');
        this.rowTemplate.filter = (item: ProtocolEntry) => this.filter(item);
        this.detailsManager = new JMSLib.DetailManager(2, 'logRowDetails');

        // Auswahl vorbereiten
        var filter = $('input[type="checkbox"]');
        filter.button();
        filter.click(() => this.refresh());

        // Geräte ermitteln
        VCRServer.ProfileCache.getPromise().then(profiles => {
            var list = $('#selProfile');

            // Alle Namen eintragen
            $.each(profiles, (index: number, profile: VCRServer.ProfileInfoContract) => list.append(new Option(profile.name)));

            // Auf Änderungen der Liste reagieren
            list.change(() => this.reload());

            // Und erstmalig laden - erst danach zeigen wir dem Anwender etwas
            this.reload(this.registerAsyncCall());

            profilesLoaded();
        });
    }
}

// Die gespeicherten Suchen
class favoritesPage extends Page implements IPage {
    title: string = 'Gespeicherte Suchen';

    visibleLinks: string = '.newLink, .planLink, .currentLink, .guideLink';

    private favorites: JMSLib.HTMLTemplate;

    private filter: JQuery;

    private static showAll: boolean = true;

    onShow(): void {
        var queries = SavedGuideQueries.load();

        this.favorites.loadList(queries);

        // Sobald die Zähler zur Verfügung stehen zweigen wir nur neu an
        SavedGuideQuery.onCountLoaded = (query: SavedGuideQuery) => this.favorites.loadList(queries);

        // Beim Löschen muss die Änderung gespeichert und die Anzeige aktualisiert werden
        SavedGuideQuery.onDeleted = (query: SavedGuideQuery) => {
            for (var i = 0; i < queries.length; i++)
                if (queries[i] === query) {
                    queries.splice(i, 1);

                    SavedGuideQueries.save();

                    this.favorites.loadList(queries);

                    break;
                }
        }

        // Auswahl übernehmen
        SavedGuideQuery.onClick = (query: SavedGuideQuery) => SavedGuideQuery.guideScope = query;
    }

    private filterFavorite(favorite: SavedGuideQuery): boolean {
        if (favoritesPage.showAll)
            return true;
        else
            return (favorite.count() != 0);
    }

    private refresh(): void {
        favoritesPage.showAll = (this.filter.find(':checked').val() == '1')

        this.favorites.refresh();
    }

    onInitialize(): void {
        SavedGuideQuery.onDeleted = null;

        // Liste mit den aktuellen Favoriten aufbauen
        this.favorites = JMSLib.HTMLTemplate.dynamicCreate($('#favoriteList'), 'favoriteRow');
        this.favorites.filter = fav => this.filterFavorite(fav);
        this.filter = $('#favoriteFilter');

        // Wir brauchen mindestens die Informationen aus dem Benutzerprofil
        VCRServer.UserProfile.global.register(this.registerAsyncCall());

        // Filter vorbereiten
        this.filter.find('input[value="' + (favoritesPage.showAll ? 1 : 0) + '"]').prop('checked', true);
        this.filter.buttonset();
        this.filter.change(() => this.refresh());
    }
}
