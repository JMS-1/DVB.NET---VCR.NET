/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    export class Home extends NoUiView<App.HomePage>{
        render(): JSX.Element {
            return <div>
                <div>
                    Willkommen zur Benutzeroberfläche des VCR.NET Recording Service. Von hier aus geht es direkt zu:
                <ul>
                        <li><InternalLink pict="plan" view="plan">dem Aufzeichnungsplan</InternalLink> mit den anstehenden Aufzeichnungen<HelpLink page="faq;parallelrecording" /></li>
                        <li><InternalLink pict="devices" view="current">den laufenden Aufzeichnungen</InternalLink> mit den Aktivitäten der einzelnen DVB Geräte</li>
                        <li><InternalLink pict="guide" view="guide">der Programmzeitschrift</InternalLink> zum Anlegen neuer Aufzeichnungen<HelpLink page="faq;epg" /></li>
                        <li><InternalLink pict="new" view="edit">einer neuen Aufzeichnung</InternalLink></li>
                    </ul>
                    <ul>
                        <li><InternalLink pict="jobs" view="jobs">den vorhandenen Aufzeichnungen</InternalLink>, um diese zu verändern oder ins Archiv zu übertragen</li>
                        <li><InternalLink view="jobs;archive">den archivierten Aufzeichnungen</InternalLink>, um diese anzusehen, zu verändern, zu reaktivieren oder endgültig zu löschen<HelpLink page="faq;archive" /></li>
                        <li><InternalLink view="log">den Protokollen</InternalLink> von bereits durchgeführten Aufzeichnungen<HelpLink page="faq;log" /></li>
                    </ul>
                    <ul>
                        <li><InternalLink pict="settings" view="settings">den individuellen Anpassungen</InternalLink> der Web Oberfläche</li>
                    </ul>
                </div>
                <div>
                    Je nach den zugeteilten Benutzerrechten können Sie darüber hinaus folgende administrative Tätigkeiten wahrnehmen:
                    <ul>
                        <li><InternalLink view="">die Programmzeitschrift sobald wie möglich aktualisieren</InternalLink><HelpLink page="faq;epgconfig" /></li>
                        <li><InternalLink view="">einen Sendersuchlauf sobald wie möglich durchführen</InternalLink><HelpLink page="faq;psiconfig" /></li>
                        <li>prüfen, ob inzwischen eine <InternalLink view="">neuere Version</InternalLink> des VCR.NET Recording Service angeboten wird</li>
                        <li><InternalLink pict="admin" view="admin">die Konfiguration des VCR.NET Recording Service verändern</InternalLink></li>
                    </ul>
                </div>
                <div>
                    Weitere Informationen zum VCR.NET Recording Service findet man hier im Bereich der <InternalLink view="faq;overview">Fragen &amp; Antworten</InternalLink>, auf
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
