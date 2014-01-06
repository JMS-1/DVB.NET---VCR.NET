/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='typings/jquerymobile/jquerymobile.d.ts' />
/// <reference path='vcrserver.ts' />
var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var VCRMobile;
(function (VCRMobile) {
    // Einfach nur einige Typwandlungen
    var jMobile = $;

    

    // Repräsentiert irgendeine Seite
    var Page = (function () {
        function Page() {
        }
        // Wird einmalig beim Erstellen der Seite aufgerufen
        Page.prototype.onCreate = function () {
            throw new Error("abstract");
        };

        // Meldet eine Seite an
        Page.prototype.registerSelf = function (templateName) {
            var me = this;

            // Wandeln und einmalig merken
            Page.instance = me;

            // Wo nötig anmelden
            $(document).on('pagecreate', '#' + templateName, function (event) {
                // Da werden wir die Daten platzieren
                me.content = $(event.currentTarget).find('[data-role="content"]');

                // Ausführen
                me.onCreate();

                // Und in die Anzeige übernehmen
                me.content.trigger('create');
            });
        };
        return Page;
    })();

    // Repräsentiert die Seite mit der Geräteübersicht
    var devicePage = (function (_super) {
        __extends(devicePage, _super);
        // Erstellt eine neue Seite
        function devicePage() {
            _super.call(this);
        }
        // Meldet eine Seite an
        devicePage.register = function (templateName) {
            new this().registerSelf(templateName);
        };

        // Wird einmalig beim Erstellen der Seite aufgerufen
        devicePage.prototype.onCreate = function () {
            var me = this;

            // Daten abrufen
            VCRServer.getPlanCurrentForMobile().done(function (data) {
                // Container anlegen
                var list = me.content.find('[data-role="collapsible-set"]');

                // Alle bisherige entfernen
                list.children().remove();

                // Elemente eintragen
                $.each(data, function (index, plan) {
                    list.append('<div data-role="collapsible"><H3>' + plan.device + '</H3>Stuff comes in here</div>');
                });
            });
        };
        return devicePage;
    })(Page);

    // Seitenbearbeitung anmelden
    devicePage.register('devicePage');

    // Benutzereinstellungen einmalig anfordern
    VCRServer.UserProfile.global.refresh();

    // Informationsdaten ermitteln
    VCRServer.getServerVersion().done(function (data) {
        $('.headline').text('VCR.NET Recording Service ' + data.version);
    });
})(VCRMobile || (VCRMobile = {}));
//# sourceMappingURL=mobile.js.map
