/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='typings/jquerymobile/jquerymobile.d.ts' />
/// <reference path='vcrserver.ts' />
// Globale Initialisierungen
$(function () {
    // Benutzereinstellungen einmalig anfordern
    VCRServer.UserProfile.global.refresh();

    // Informationsdaten ermitteln
    VCRServer.getServerVersion().done(function (data) {
        $('#headline').text('VCR.NET Recording Service ' + data.version);
    });
});
//# sourceMappingURL=mobile.js.map
