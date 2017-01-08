/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {

    // Die React.Js Komponente zur Anzeige der Startseite.
    export class Home extends NoUiView<App.NoUi.HomePage>{
        render(): JSX.Element {
            return <div>
                <div>
                    Willkommen zur Benutzeroberfläche des VCR.NET Recording Service. Von hier aus geht es direkt zu:
                    <ul>
                        <li><InternalLink pict="plan" view={this.props.noui.application.planPage.route}>dem Aufzeichnungsplan</InternalLink> mit den anstehenden Aufzeichnungen<HelpLink page={this.props.noui} topic="parallelrecording" /></li>
                        <li><InternalLink pict="devices" view="current">den laufenden Aufzeichnungen</InternalLink> mit den Aktivitäten der einzelnen DVB Geräte</li>
                        <li><InternalLink pict="guide" view="guide">der Programmzeitschrift</InternalLink> zum Anlegen neuer Aufzeichnungen<HelpLink page={this.props.noui} topic="epg" /></li>
                        <li><InternalLink pict="new" view={this.props.noui.application.editPage.route}>einer neuen Aufzeichnung</InternalLink></li>
                    </ul>
                    <ul>
                        <li><InternalLink pict="jobs" view="jobs">den vorhandenen Aufzeichnungen</InternalLink>, um diese zu verändern oder ins Archiv zu übertragen</li>
                        <li><InternalLink view="jobs;archive">den archivierten Aufzeichnungen</InternalLink>, um diese anzusehen, zu verändern, zu reaktivieren oder endgültig zu löschen<HelpLink page={this.props.noui} topic="archive" /></li>
                        <li><InternalLink view="log">den Protokollen</InternalLink> von bereits durchgeführten Aufzeichnungen<HelpLink page={this.props.noui} topic="log" /></li>
                    </ul>
                    <ul>
                        <li><InternalLink pict="settings" view="settings">den individuellen Anpassungen</InternalLink> der Web Oberfläche</li>
                    </ul>
                </div>
                <div>
                    Je nach den zugeteilten Benutzerrechten können Sie darüber hinaus folgende administrative Tätigkeiten wahrnehmen:
                    <ul>
                        <li><InternalLink view="">die Programmzeitschrift sobald wie möglich aktualisieren</InternalLink><HelpLink page={this.props.noui} topic="epgconfig" /></li>
                        <li><InternalLink view="">einen Sendersuchlauf sobald wie möglich durchführen</InternalLink><HelpLink page={this.props.noui} topic="psiconfig" /></li>
                        <li>prüfen, ob inzwischen eine <InternalLink view="">neuere Version</InternalLink> des VCR.NET Recording Service angeboten wird</li>
                        <li><InternalLink pict="admin" view="admin">die Konfiguration des VCR.NET Recording Service verändern</InternalLink></li>
                    </ul>
                </div>
                <div>
                    Weitere Informationen zum VCR.NET Recording Service findet man hier im Bereich der <InternalLink view={`${this.props.noui.application.helpPage.route};overview`}>Fragen &amp; Antworten</InternalLink>, auf
                    der <ExternalLink url="http://www.psimarron.net/vcrnet">Homepage im Internet</ExternalLink> oder
                    im <ExternalLink url="http://www.watchersnet.de/Default.aspx?tabid=52&g=topics&f=17">offiziellen Forum</ExternalLink>.
                </div>
                <div>
                    Dr. Jochen Manns, 2003-17
                </div>
            </div>;
        }
    }

}
