/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='typings/jquerymobile/jquerymobile.d.ts' />
/// <reference path='vcrserver.ts' />
/// <reference path='jmslib.ts' />

module VCRMobile {
    // Einfach nur einige Typwandlungen
    var jMobile = <JQueryMobile.JQueryStatic> <any> $;

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
            item.start = JMSLib.DateFormatter.getShortDate(start) + '. ' + JMSLib.DateFormatter.getEndTime(start);
            item.onShowDetails = function (event: JQueryEventObject): void { item.showDetails($(event.target).parent()); };
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

        // Zeigt die Detailinformationen an
        private showDetails(container: JQuery): void {
            var parent = container.parent();

            // Schaltfläche entfernen
            container.remove();

            // Details anfordern
            VCRServer.getGuideItem(this.profileName, this.source, this.startTime, this.endTime).done(function (item: VCRServer.GuideItemContract): void {
                if (item != null)
                    parent.append(JMSLib.HTMLTemplate.cloneAndApplyTemplate(item, $('#guideDetails')));
            });
        }
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
        onCreate(): void { throw new Error("abstract"); }

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

    // Seitenbearbeitung anmelden
    devicePage.register('devicePage');

    // Benutzereinstellungen einmalig anfordern
    VCRServer.UserProfile.global.refresh();

    // Informationsdaten ermitteln
    VCRServer.getServerVersion().done(function (data: VCRServer.InfoServiceContract): void {
        $('.headline').text('VCR.NET Recording Service ' + data.version);
    });
}