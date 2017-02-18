/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class DvbNet extends HelpComponent {
        readonly title = "DVB.NET";

        render(page: App.IPage): JSX.Element {
            return <div>
                Der VCR.NET Recording Service ist ausschließlich der Koordinator von Aufzeichnungen.
                Die eigentlichen Aktivitäten<JMSLib.ReactUi.InternalLink view={page.application.devicesPage.route} pict="devices" /> auf
                den DVB Geräten werden mit Hilfe
                der <JMSLib.ReactUi.ExternalLink url="http://www.psimarron.net/DVBNET/">DVB.NET Bibliothek</JMSLib.ReactUi.ExternalLink> ausgeführt.
                Insbesondere muss für jedes DVB Gerät, das der VCR.NET Recording Service
                nutzen soll,
                ein <JMSLib.ReactUi.ExternalLink url="http://www.psimarron.net/DVBNET/html/profiles.html">DVB.NET Geräteprofil</JMSLib.ReactUi.ExternalLink> angelegt
                und korrekt konfiguriert werden. Es wird empfohlen,
                sich mit den grundsätzlichen Konzepten von DVB.NET vertraut zu machen, um zu verstehen,
                wie der VCR.NET Recording Service auf die vorhandene Hardware zugreift. Insbesondere
                für die <JMSLib.ReactUi.ExternalLink url="http://www.psimarron.net/DVBNET/html/sourcescan.html">Liste der Quellen und deren Aktualisierung</JMSLib.ReactUi.ExternalLink> ist
                eine korrekte Konfiguration von vitaler Bedeutung.
            </div>;
        }
    }
}
