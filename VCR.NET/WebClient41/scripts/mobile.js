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
            item.onShowDetails = function (event) {
                item.showDetails($(event.target).parent());
            };
            item.hideDetails = serverItem.epg ? '' : JMSLib.CSSClass.invisible;
            item.end = JMSLib.DateFormatter.getEndTime(end);
            item.sourceName = serverItem.sourceName;
            item.profileName = serverItem.device;
            item.source = serverItem.source;
            item.name = serverItem.name;
            item.startTime = start;
            item.endTime = end;

            return item;
        };

        // Zeigt die Detailinformationen an
        CurrentItem.prototype.showDetails = function (container) {
            var parent = container.parent();

            // Schaltfläche entfernen
            container.remove();

            // Details anfordern
            VCRServer.getGuideItem(this.profileName, this.source, this.startTime, this.endTime).done(function (item) {
                if (item != null)
                    parent.append(JMSLib.HTMLTemplate.cloneAndApplyTemplate(item, $('#guideDetails')));
            });
        };
        return CurrentItem;
    })();

    // Beschreibt einen einzelnen Eintrag in Aufzeichnungsplan
    var PlanItem = (function () {
        function PlanItem() {
        }
        PlanItem.create = function (rawData) {
            var item = new PlanItem();

            // Zeiten umrechnen
            var duration = rawData.duration * 1000;
            var start = new Date(rawData.start);
            var end = new Date(start.getTime() + duration);

            // Daten aus der Rohdarstellung in das Modell kopieren
            item.displayStart = JMSLib.DateFormatter.getShortDate(start) + '. ' + JMSLib.DateFormatter.getEndTime(start);
            item.station = (rawData.station == null) ? '(Aufzeichnung gelöscht)' : rawData.station;
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
                item.mode = 'lost';
            else if (rawData.late)
                item.mode = 'late';
            else
                item.mode = 'intime';

            // Die Endzeit könnte nicht wie gewünscht sein
            if (rawData.suspectEndTime)
                item.endTimeSuspect = CSSClass.badEndTime;

            return item;
        };
        return PlanItem;
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

    // Repräsentiert die Seite mit dem Aufzeichnungsplan
    var planPage = (function (_super) {
        __extends(planPage, _super);
        // Erstellt eine neue Seite
        function planPage() {
            _super.call(this);
            // Die Vorlage für einen einzelnen Eintrag
            this.rowTemplate = null;
        }
        // Meldet eine Seite an
        planPage.register = function (templateName) {
            new this().registerSelf(templateName);
        };

        // Wird einmalig beim Erstellen der Seite aufgerufen
        planPage.prototype.onCreate = function () {
            var me = this;

            // Aktualisierung anmelden
            me.content.find('.refreshLink').on('click', function (eventObject) {
                me.refresh();
            });

            // Vorlage einmalig anlegen und Daten erstmalig anfordern
            me.rowTemplate = JMSLib.HTMLTemplate.staticCreate(me.content.find('[data-role="collapsible-set"]'), $('#planRow'));
            me.refresh();
        };

        // Fordert alle Daten (erneut) an
        planPage.prototype.refresh = function () {
            var me = this;

            // Daten abrufen
            VCRServer.getPlanForMobile(20).done(function (data) {
                // Daten aktualisieren
                me.rowTemplate.loadList($.map(data, PlanItem.create));

                // Und in die Anzeige übernehmen
                me.content.trigger('create');
            });
        };
        return planPage;
    })(Page);

    // Seitenbearbeitung anmelden
    devicePage.register('devicePage');
    planPage.register('planPage');

    // Benutzereinstellungen einmalig anfordern
    VCRServer.UserProfile.global.refresh();

    // Informationsdaten ermitteln
    VCRServer.getServerVersion().done(function (data) {
        $('.headline').text('VCR.NET Recording Service ' + data.version);
    });
})(VCRMobile || (VCRMobile = {}));
//# sourceMappingURL=mobile.js.map
