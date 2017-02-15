/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // Die React.Js Komponente zur Anzeige der Startseite.
    export class Home extends JMSLib.ReactUi.Component<App.IHomePage>{
        render(): JSX.Element {
            var application = this.props.noui.application;

            return <div className="vcrnet-home">
                <div>
                    Willkommen zur Benutzeroberfläche des VCR.NET Recording Service. Von hier aus geht es direkt zu:
                    <ul>
                        <li><JMSLib.ReactUi.InternalLink pict="plan" view={application.planPage.route}>dem Aufzeichnungsplan</JMSLib.ReactUi.InternalLink> mit den anstehenden Aufzeichnungen<HelpLink page={this.props.noui} topic="parallelrecording" /></li>
                        <li><JMSLib.ReactUi.InternalLink pict="devices" view="current">den laufenden Aufzeichnungen</JMSLib.ReactUi.InternalLink> mit den Aktivitäten der einzelnen DVB Geräte</li>
                        <li><JMSLib.ReactUi.InternalLink pict="guide" view={application.guidePage.route}>der Programmzeitschrift</JMSLib.ReactUi.InternalLink> zum Anlegen neuer Aufzeichnungen<HelpLink page={this.props.noui} topic="epg" /></li>
                        <li><JMSLib.ReactUi.InternalLink pict="new" view={application.editPage.route}>einer neuen Aufzeichnung</JMSLib.ReactUi.InternalLink></li>
                    </ul>
                    <ul>
                        <li><JMSLib.ReactUi.InternalLink pict="jobs" view={application.jobPage.route}>den vorhandenen Aufzeichnungen</JMSLib.ReactUi.InternalLink>, um diese zu verändern oder ins Archiv zu übertragen</li>
                        <li><JMSLib.ReactUi.InternalLink view={`${application.jobPage.route};archive`}>den archivierten Aufzeichnungen</JMSLib.ReactUi.InternalLink>, um diese anzusehen, zu verändern, zu reaktivieren oder endgültig zu löschen<HelpLink page={this.props.noui} topic="archive" /></li>
                        <li><JMSLib.ReactUi.InternalLink view={application.logPage.route}>den Protokollen</JMSLib.ReactUi.InternalLink> von bereits durchgeführten Aufzeichnungen<HelpLink page={this.props.noui} topic="log" /></li>
                    </ul>
                    <ul>
                        <li><JMSLib.ReactUi.InternalLink pict="settings" view={application.settingsPage.route}>den individuellen Anpassungen</JMSLib.ReactUi.InternalLink> der Web Oberfläche</li>
                    </ul>
                </div>
                <div>
                    Je nach den zugeteilten Benutzerrechten können Sie darüber hinaus folgende administrative Tätigkeiten wahrnehmen:
                    <ul>
                        <li><JMSLib.ReactUi.InternalLink view="">die Programmzeitschrift sobald wie möglich aktualisieren</JMSLib.ReactUi.InternalLink><HelpLink page={this.props.noui} topic="epgconfig" /></li>
                        <li><JMSLib.ReactUi.InternalLink view="">einen Sendersuchlauf sobald wie möglich durchführen</JMSLib.ReactUi.InternalLink><HelpLink page={this.props.noui} topic="psiconfig" /></li>
                        <li>prüfen, ob inzwischen eine <JMSLib.ReactUi.InternalLink view="">neuere Version</JMSLib.ReactUi.InternalLink> des VCR.NET Recording Service angeboten wird</li>
                        <li><JMSLib.ReactUi.InternalLink pict="admin" view={application.adminPage.route}>die Konfiguration des VCR.NET Recording Service verändern</JMSLib.ReactUi.InternalLink></li>
                    </ul>
                </div>
                <div>
                    Weitere Informationen zum VCR.NET Recording Service findet man hier im Bereich der <JMSLib.ReactUi.InternalLink view={`${application.helpPage.route};overview`}>Fragen &amp; Antworten</JMSLib.ReactUi.InternalLink>, auf
                    der <JMSLib.ReactUi.ExternalLink url="http://www.psimarron.net/vcrnet">Homepage im Internet</JMSLib.ReactUi.ExternalLink> oder
                    im <JMSLib.ReactUi.ExternalLink url="http://www.watchersnet.de/Default.aspx?tabid=52&g=topics&f=17">offiziellen Forum</JMSLib.ReactUi.ExternalLink>.
                </div>
                <div>
                    Dr. Jochen Manns, 2003-17
                </div>
            </div>;
        }
    }

}
