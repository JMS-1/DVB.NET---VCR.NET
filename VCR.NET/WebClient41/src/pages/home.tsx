/// <reference path="../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // Die React.Js Komponente zur Anzeige der Startseite.
    export class Home extends JMSLib.ReactUi.ComponentWithSite<App.IHomePage>{
        render(): JSX.Element {
            var application = this.props.noui.application;
            var showGuide = this.props.noui.showStartGuide;
            var showScan = this.props.noui.showStartScan;

            return <div className="vcrnet-home">
                <div>
                    Willkommen zur Benutzeroberfläche des VCR.NET Recording Service. Von hier aus geht es direkt zu:
                    <ul>
                        <li><JMSLib.ReactUi.InternalLink pict="plan" view={application.planPage.route}>dem Aufzeichnungsplan</JMSLib.ReactUi.InternalLink> mit den anstehenden Aufzeichnungen<HelpLink page={this.props.noui} topic="parallelrecording" /></li>
                        <li><JMSLib.ReactUi.InternalLink pict="devices" view={application.devicesPage.route}>den laufenden Aufzeichnungen</JMSLib.ReactUi.InternalLink> mit den Aktivitäten der einzelnen DVB Geräte</li>
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
                        <li>{showGuide.isReadonly ? showGuide.text : <JMSLib.ReactUi.InternalLink view={() => showGuide.value = !showGuide.value}>{showGuide.text}</JMSLib.ReactUi.InternalLink>}<HelpLink page={this.props.noui} topic="epgconfig" /></li>
                        {showGuide.value ? <Task noui={this.props.noui.startGuide}>
                            Mit der Schaltfläche unter diesem Text kann eine baldmögliche Aktualisierung der
                            Programmzeitschrift<HelpLink topic="epg" page={this.props.noui} /> angefordert
                            werden. Sind gerade Aufzeichnungen aktiv oder in nächster Zeit geplant, so wird der VCR.NET
                            Recording Service die Aktualisierung auf den nächstmöglichen Zeitpunkt verschieben, da die Ausführung
                            regulärer Aufzeichnungen immer Priorität vor allen Aktualisierungen
                            hat.<HelpLink topic="tasks" page={this.props.noui} />
                        </Task> : null}
                        <li>{showScan.isReadonly ? showScan.text : <JMSLib.ReactUi.InternalLink view={() => showScan.value = !showScan.value}>{showScan.text}</JMSLib.ReactUi.InternalLink>}<HelpLink page={this.props.noui} topic="psiconfig" /></li>
                        {showScan.value ? <Task noui={this.props.noui.startScan}>
                            Hier ist es nun möglich, die Aktualisierung der Quellen der vom VCR.NET Recording Service verwalteten
                            DVB.NET Geräte anzufordern. Da die Aktualisierung der Quellen eine niedrigere Priorität besitzt als
                            die Ausführung regulärer Aufzeichnungen kann es sein, dass sie nicht unmittelbar gestartet wird.
                            Der VCR.NET Recording Service wird dies aber bei nächster Gelegenheit
                            nachholen.<HelpLink topic="tasks" page={this.props.noui} />
                        </Task> : null}
                        <li>prüfen, ob inzwischen eine <JMSLib.ReactUi.InternalLink view="">neuere Version</JMSLib.ReactUi.InternalLink> des VCR.NET Recording Service angeboten wird</li>
                        <li>{this.props.noui.isRecording ? "die Konfiguration des VCR.NET Recording Service verändern" : <JMSLib.ReactUi.InternalLink pict="admin" view={application.adminPage.route}>die Konfiguration des VCR.NET Recording Service verändern</JMSLib.ReactUi.InternalLink>}</li>
                    </ul>
                </div>
                {this.props.noui.isRecording ? <div>
                    Hinweis: Der VCR.NET Recording Service führt gerade eine oder mehrere Aufzeichnungen
                    oder Aktualisierungen von Programmzeitschrift respektive Senderliste aus.<JMSLib.ReactUi.InternalLink view={this.props.noui.application.devicesPage.route} pict="info" />
                </div> : null}
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
