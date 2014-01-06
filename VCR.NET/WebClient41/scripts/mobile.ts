/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='typings/jquerymobile/jquerymobile.d.ts' />
/// <reference path='vcrserver.ts' />

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

                // Und in die Anzeige übernehmen
                me.content.trigger('create');
            });
        }
    }

    // Repräsentiert die Seite mit der Geräteübersicht
    class devicePage extends Page {
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

            // Daten abrufen
            VCRServer.getPlanCurrentForMobile().done(function (data: VCRServer.PlanCurrentContractMobile[]): void {
                // Container anlegen
                var list = me.content.find('[data-role="collapsible-set"]');

                // Alle bisherige entfernen
                list.children().remove();

                // Elemente eintragen
                $.each(data, function (index: number, plan: VCRServer.PlanCurrentContractMobile): void {
                    list.append('<div data-role="collapsible"><H3>' + plan.device + '</H3>Stuff comes in here</div>');
                });
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