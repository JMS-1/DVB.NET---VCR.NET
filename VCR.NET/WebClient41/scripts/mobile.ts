/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='typings/jquerymobile/jquerymobile.d.ts' />
/// <reference path='vcrserver.ts' />
/// <reference path='jmslib.ts' />

module VCRMobile {
    // Einfach nur einige Typwandlungen
    var jMobile = <JQueryMobile.JQueryStatic> <any> $;

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
                me.rowTemplate.loadList(data);

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