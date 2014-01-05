/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='typings/jquerymobile/jquerymobile.d.ts' />
/// <reference path='vcrserver.ts' />
var jMobile = $;

// Globale Initialisierungen
$(function () {
    // Seitenbearbeitung anmelden
    $(document).on('pagecreate', '#devicePage', function (event) {
        // Da werden wir die Daten platzieren
        var content = $(event.currentTarget).find('[data-role="content"]');

        // Daten abrufen
        VCRServer.getPlanCurrentForMobile().done(function (data) {
            // Container anlegen
            var list = $('<div data-role="collapsible-set"/>');

            // Elemente eintragen
            $.each(data, function (index, plan) {
                list.append('<div data-role="collapsible"><H3>' + plan.device + '</H3>Stuff comes in here</div>');
            });

            // Und in die Anzeige übernehmen
            content.find('[data-role="collapsible-set"]').remove();
            content.prepend(list);
            content.trigger('create');
        });
    });

    // Benutzereinstellungen einmalig anfordern
    VCRServer.UserProfile.global.refresh();

    // Informationsdaten ermitteln
    VCRServer.getServerVersion().done(function (data) {
        $('.headline').text('VCR.NET Recording Service ' + data.version);
    });
});
//# sourceMappingURL=mobile.js.map
