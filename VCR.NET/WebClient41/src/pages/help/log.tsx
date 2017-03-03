/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class Log extends HelpComponent {
        readonly title = "Protokolle";

        render(page: App.IPage): JSX.Element {
            return <div>
                Der VCR.NET Recording Service erstellt für jede ausgeführte
                Aktivität<JMSLib.ReactUi.InternalLink view={page.application.devicesPage.route} pict="devices" /> einen
                gesonderten Protokolleintrag. Eine Aktivität ist hierbei
                immer ein geschlossener Nutzungszeitraum eines DVB Gerätes. In diesem Zeitraum kann
                entweder eine einzelne Sonderaufgabe oder
                eine <JMSLib.ReactUi.InternalLink view={`${page.route};parallelrecording`}>Gruppe von Aufzeichnungen</JMSLib.ReactUi.InternalLink> ausgeführt
                worden sein. Die <JMSLib.ReactUi.InternalLink view={page.application.logPage.route}>Protokolleinträge</JMSLib.ReactUi.InternalLink> können
                direkt über die Web Oberfläche abgerufen werden.
                <br />
                <br />
                In der Liste sieht man immer die Einträge einer Woche für ein einzelnes DVB Geräte.
                Über Auswahllisten ist es möglich, sowohl den Zeitraum als auch das betrachtete
                Gerät festzulegen. Die
                Konfiguration<JMSLib.ReactUi.InternalLink view={`${page.application.adminPage.route};other`} pict="admin" /> des
                VCR.NET Recording Service sieht vor, dass veraltete Protokolleinträge automatisch
                entfernt werden. Man beachte aber, dass dies nicht periodisch geschieht, sondern
                nur bei Aufruf der Liste über die Web Oberfläche.
                <br />
                <br />
                Erst einmal werden nur die Protokolleinträge angezeigt, die von Aufzeichnungen erzeugt
                wurden. Auf Wunsch ist es möglich, auch die Einträge
                der <JMSLib.ReactUi.InternalLink view={`${page.route};tasks`}>Sonderaufgaben</JMSLib.ReactUi.InternalLink> hinzu
                zu schalten - einzeln pro Art der Aufgabe.
                <br />
                <br />
                Wählt man einen einzelnen Eintrag an, so wird eine Detailanzeige geöffnet. In dieser
                lässt sich für Aufzeichnungen zum Beispiel die gesamte Größe aller zugehörigen Aufzeichnungsdateien
                sowie Beginn und Ende der Aktivität über alle enthaltenen Aufzeichnungen ablesen.
                <br />
                <br />
                Sind die <JMSLib.ReactUi.InternalLink view={`${page.route};filecontents`}>Aufzeichnungsdateien</JMSLib.ReactUi.InternalLink> zu einem Eintrag noch
                vorhanden, so wird dies durch kleine Symbole <JMSLib.ReactUi.Pictogram name="recording" /> bei dem Eintrag visualisiert.
                Ist auf dem Rechner, von dem die Web Oberfläche aufgerufen wurde,
                der <JMSLib.ReactUi.ExternalLink url="http://www.psimarron.net/DVBNETViewer/">DVB.NET / VCR.NET Viewer</JMSLib.ReactUi.ExternalLink> lokal
                installiert, so kann durch Klicken auf ein
                Symbol die zugehörige Aufzeichnungsdatei zur Anzeige geöffnet werden.
            </div>;
        }
    }
}
