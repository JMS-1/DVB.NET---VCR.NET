/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='typings/jquerymobile/jquerymobile.d.ts' />
/// <reference path='vcrserver.ts' />
var jMobile = $;

// Globale Initialisierungen
$(function () {
    // Seitenbearbeitung anmelden
    $(document).on('pagebeforecreate', '#devicePage', function (event) {
        var content = $(event.currentTarget).find('.ui-content');

        VCRServer.getPlanCurrent().done(function (data) {
            content.prepend('<ul data-role="listview" data-inset="true"><li><a href="#">DVB-S2</a></li><li><a href="#">Nexus</a></li></ul>');
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
