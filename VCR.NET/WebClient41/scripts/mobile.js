/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='typings/jquerymobile/jquerymobile.d.ts' />
/// <reference path='vcrserver.ts' />
/// <reference path='jmslib.ts' />
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

    // Die Information zu einer einzelnen laufenden Aufzeichnung
    var CurrentItem = (function () {
        function CurrentItem() {
        }
        CurrentItem.create = function (serverItem) {
            var item = new CurrentItem();

            // Zeiten umrechnen
            var duration = serverItem.duration * 1000;
            var start = new Date(serverItem.start);
            var end = new Date(start.getTime() + duration);

            // Einfache Übernahme
            item.start = JMSLib.DateFormatter.getShortDate(start) + '. ' + JMSLib.DateFormatter.getEndTime(start);
            item.end = JMSLib.DateFormatter.getEndTime(end);
            item.sourceName = serverItem.sourceName;
            item.profileName = serverItem.device;
            item.name = serverItem.name;

            return item;
        };
        return CurrentItem;
    })();

    

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
            // Die Vorlage für einen einzelnen Eintrag
            this.rowTemplate = null;
        }
        // Meldet eine Seite an
        devicePage.register = function (templateName) {
            new this().registerSelf(templateName);
        };

        // Wird einmalig beim Erstellen der Seite aufgerufen
        devicePage.prototype.onCreate = function () {
            var me = this;

            // Aktualisierung anmelden
            me.content.find('.refreshLink').on('click', function (eventObject) {
                me.refresh();
            });

            // Vorlage einmalig anlegen und Daten erstmalig anfordern
            me.rowTemplate = JMSLib.HTMLTemplate.staticCreate(me.content.find('[data-role="collapsible-set"]'), $('#deviceRow'));
            me.refresh();
        };

        // Fordert alle Daten (erneut) an
        devicePage.prototype.refresh = function () {
            var me = this;

            // Daten abrufen
            VCRServer.getPlanCurrentForMobile().done(function (data) {
                // Daten aktualisieren
                me.rowTemplate.loadList($.map(data, CurrentItem.create));

                // Und in die Anzeige übernehmen
                me.content.trigger('create');
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
