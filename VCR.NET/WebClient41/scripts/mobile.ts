/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='typings/jquerymobile/jquerymobile.d.ts' />
/// <reference path='vcrserver.ts' />
/// <reference path='jmslib.ts' />

module VCRMobile {
    // Einfach nur einige Typwandlungen
    var jMobile = <JQueryMobile.JQueryStatic> <any> $;

    // Zeigt einen Eintrag der Programmzeitschrift an
    function displayGuide(container: JQuery, profile: string, source: string, start: Date, end: Date): void {
        var parent = container.parent();

        // Schaltfläche entfernen
        container.remove();

        // Details anfordern
        VCRServer.getGuideItem(profile, source, start, end).done(function (item: VCRServer.GuideItemContract): void {
            if (item == null)
                return;

            // Anzeigeelement anlegen
            var guide = JMSLib.HTMLTemplate.cloneAndApplyTemplate(item, $('#guideDetails'));

            // Eine kleine Hilfsklasse zum Aufbau der Überlappanzeige
            var guideItem: JMSLib.IGuideItem = {
                overlapClass: JMSLib.CSSClass.partialRecord,
                shortDescription: item.shortDescription,
                duration: item.duration * 1000,
                description: item.description,
                start: new Date(item.start),
            };

            // Überlapp einblenden
            parent.append(JMSLib.prepareGuideDisplay(guideItem, guide, start, end));
        });
    }

    // Die Information zu einer einzelnen laufenden Aufzeichnung
    class CurrentItem {
        // Das verwendete Gerät
        profileName: string;

        // Der Name der Aufzeichnung
        name: string;

        // Der Startzeitpunkt der Aufzeichnung
        start: string;

        // Das Ende der Aufzeichnung
        end: string;

        // Der Startzeitpunkt der Aufzeichnung
        private startTime: Date;

        // Das Ende der Aufzeichnung
        private endTime: Date;

        // Der zugehörige Sender
        private source: string;

        // Der zugehörige Sender
        sourceName: string;

        // Gesetzt, wenn die Detailanzeige nicht möglich ist
        hideDetails: string;

        // Über diese Methode werden die Detailinformationen geladen
        onShowDetails: (event: JQueryEventObject) => void;

        static create(serverItem: VCRServer.PlanCurrentContractMobile): CurrentItem {
            var item = new CurrentItem();

            // Zeiten umrechnen
            var duration = serverItem.duration * 1000;
            var start = new Date(serverItem.start);
            var end = new Date(start.getTime() + duration);

            // Einfache Übernahme
            item.onShowDetails = function (event: JQueryEventObject): void { displayGuide($(event.target).parent(), item.profileName, item.source, item.startTime, item.endTime); };
            item.start = JMSLib.DateFormatter.getShortDate(start) + '. ' + JMSLib.DateFormatter.getEndTime(start);
            item.hideDetails = serverItem.epg ? '' : JMSLib.CSSClass.invisible;
            item.end = JMSLib.DateFormatter.getEndTime(end);
            item.sourceName = serverItem.sourceName;
            item.profileName = serverItem.device;
            item.source = serverItem.source;
            item.name = serverItem.name;
            item.startTime = start;
            item.endTime = end;

            return item;
        }
    }

    // Beschreibt einen einzelnen Eintrag in Aufzeichnungsplan
    class PlanItem {
        static create(rawData: any): PlanItem {
            var item = new PlanItem();

            // Zeiten umrechnen
            var duration = rawData.duration * 1000;
            var start = new Date(rawData.start);
            var end = new Date(start.getTime() + duration);

            // Daten aus der Rohdarstellung in das Modell kopieren
            item.onShowDetails = function (event: JQueryEventObject): void { displayGuide($(event.target).parent(), item.profile, item.source, item.start, item.end); };
            item.displayStart = JMSLib.DateFormatter.getShortDate(start) + '. ' + JMSLib.DateFormatter.getEndTime(start);
            item.station = (rawData.station == null) ? '(Aufzeichnung gelöscht)' : rawData.station;
            item.hideDetails = rawData.epg ? '' : JMSLib.CSSClass.invisible;
            item.profile = (rawData.device == null) ? '' : rawData.device;
            item.displayEnd = JMSLib.DateFormatter.getEndTime(end);
            item.epgProfile = rawData.epgDevice;
            item.fullName = rawData.name;
            item.source = rawData.source;
            item.legacyId = rawData.id;
            item.start = start;
            item.end = end;

            // Aufzeichungsmodus ermitteln
            if (rawData.lost)
                item.modeIcon = 'delete';
            else if (rawData.late)
                item.modeIcon = 'alert';
            else
                item.modeIcon = 'check';

            item.iconClass = 'iconMerge ui-btn-icon-left ui-icon-' + item.modeIcon;

            // Die Endzeit könnte nicht wie gewünscht sein
            if (rawData.suspectEndTime)
                item.endTimeSuspect = CSSClass.badEndTime;

            return item;
        }

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

        // Ein Kürzel für die Qualität der Aufzeichnung, etwa ob dieser verspätet beginnt.
        modeIcon: string;

        // Die CSS Klasse zu modeIcon.
        iconClass: string;

        // Gesetzt, wenn die Detailanzeige nicht möglich ist
        hideDetails: string;

        // Über diese Methode werden die Detailinformationen geladen
        onShowDetails: (event: JQueryEventObject) => void;
    }

    // Repräsentiert irgendeine Seite
    interface IPage {
    }

    // Repräsentiert irgendeine Seite
    class Page implements IPage {
        // Die einzige Instanz dieser Klasse
        static instance: IPage;

        // Der eigentliche Inhalt
        content: JQuery;

        // Wird einmalig beim Erstellen der Seite aufgerufen
        onCreate(): void { throw new Error('abstract'); }

        // Meldet eine Seite an
        registerSelf(templateName: string): void {
            var me = this;

            // Wandeln und einmalig merken
            Page.instance = me;

            // Wo nötig anmelden
            $(document).on('pagecreate', '#' + templateName, function (event: JQueryEventObject): void {
                // Da werden wir die Daten platzieren
                me.content = $(event.currentTarget).find('[data-role="content"]');

                // Ausführen
                me.onCreate();
            });
        }
    }

    // Repräsentiert die Seite mit der Geräteübersicht
    class devicePage extends Page {
        // Die Vorlage für einen einzelnen Eintrag
        private rowTemplate: JMSLib.HTMLTemplate = null;

        // Erstellt eine neue Seite
        constructor() {
            super();
        }

        // Meldet eine Seite an
        static register(templateName: string): void {
            new this().registerSelf(templateName);
        }

        // Wird einmalig beim Erstellen der Seite aufgerufen
        onCreate(): void {
            var me = this;

            // Aktualisierung anmelden
            me.content.find('.refreshLink').on('click', function (eventObject: JQueryEventObject): void {
                me.refresh();
            });

            // Vorlage einmalig anlegen und Daten erstmalig anfordern
            me.rowTemplate = JMSLib.HTMLTemplate.staticCreate(me.content.find('[data-role="collapsible-set"]'), $('#deviceRow'));
            me.refresh();
        }

        // Fordert alle Daten (erneut) an
        refresh(): void {
            var me = this;

            // Daten abrufen
            VCRServer.getPlanCurrentForMobile().done(function (data: VCRServer.PlanCurrentContractMobile[]): void {
                // Daten aktualisieren
                me.rowTemplate.loadList($.map(data, CurrentItem.create));

                // Und in die Anzeige übernehmen
                me.content.trigger('create');
            });
        }
    }

    // Repräsentiert die Seite mit dem Aufzeichnungsplan
    class planPage extends Page {
        // Die Vorlage für einen einzelnen Eintrag
        private rowTemplate: JMSLib.HTMLTemplate = null;

        // Erstellt eine neue Seite
        constructor() {
            super();
        }

        // Meldet eine Seite an
        static register(templateName: string): void {
            new this().registerSelf(templateName);
        }

        // Wird einmalig beim Erstellen der Seite aufgerufen
        onCreate(): void {
            var me = this;

            // Aktualisierung anmelden
            me.content.find('.refreshLink').on('click', function (eventObject: JQueryEventObject): void {
                me.refresh();
            });

            // Vorlage einmalig anlegen und Daten erstmalig anfordern
            me.rowTemplate = JMSLib.HTMLTemplate.staticCreate(me.content.find('[data-role="listview"]'), $('#planRow'));
            me.refresh();
        }

        // Fordert alle Daten (erneut) an
        refresh(): void {
            var me = this;

            // Daten abrufen
            VCRServer.getPlanForMobile(20).done(function (data: VCRServer.PlanCurrentContractMobile[]): void {
                // Daten aktualisieren
                me.rowTemplate.loadList($.map(data, PlanItem.create));

                // Und in die Anzeige übernehmen
                me.content.trigger('create');
            });
        }
    }

    // Seitenbearbeitung anmelden
    devicePage.register('devicePage');
    planPage.register('planPage');

    // Benutzereinstellungen einmalig anfordern
    VCRServer.UserProfile.global.refresh();

    // Informationsdaten ermitteln
    VCRServer.getServerVersion().done(function (data: VCRServer.InfoServiceContract): void {
        $('.headline').text('VCR.NET Recording Service ' + data.version);
    });
}