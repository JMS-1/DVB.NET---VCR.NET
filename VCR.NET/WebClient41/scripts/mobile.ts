/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='typings/jquerymobile/jquerymobile.d.ts' />
/// <reference path='vcrserver.ts' />

var jMobile = <JQueryMobile.JQueryStatic> <any> $;

// Globale Initialisierungen
$(function (): void {
    // Seitenbearbeitung anmelden
    $(document).on('pagecreate', '#devicePage', function (event: JQueryEventObject): void {
        // Da werden wir die Daten platzieren
        var content = $(event.currentTarget).find('[data-role="content"]');

        // Daten abrufen
        VCRServer.getPlanCurrentForMobile().done(function (data: VCRServer.PlanCurrentContractMobile[]): void {
            // Container anlegen
            var list = $('<div data-role="collapsible-set"/>');

            // Elemente eintragen
            $.each(data, function (index: number, plan: VCRServer.PlanCurrentContractMobile): void {
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
    VCRServer.getServerVersion().done(function (data: VCRServer.InfoServiceContract): void {
        $('.headline').text('VCR.NET Recording Service ' + data.version);
    });
});