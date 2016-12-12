/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IHomeStatic {
    }

    interface IHomeDynamic {
    }

    export class Home extends React.Component<IHomeStatic, IHomeDynamic>{
        render(): JSX.Element {
            return <div>
                <div>
                    Willkommen zur Benutzeroberfläche des VCR.NET Recording Service. Von hier aus geht es direkt zu:
                <ul>
                        <li><InternalLink text="dem Aufzeichnungsplan" view="plan" /> mit den anstehenden Aufzeichnungen<HelpLink page="faq;parallelrecording"/></li>
                        <li><InternalLink text="den laufenden Aufzeichnungen" view="current" /> mit den Aktivitäten der einzelnen DVB Geräte</li>
                        <li><InternalLink text="der Programmzeitschrift" view="guide" /> zum Anlegen neuer Aufzeichnungen<HelpLink page="faq;epg" /></li>
                        <li><InternalLink text="einer neuen Aufzeichnung" view="edit" /></li>
                    </ul>
                    <ul>
                        <li><InternalLink text="den vorhandenen Aufzeichnungen" view="jobs" />, um diese zu verändern oder ins Archiv zu übertragen</li>
                        <li><InternalLink text="den archivierten Aufzeichnungen" view="jobs;archive" />, um diese anzusehen, zu verändern, zu reaktivieren oder endgültig zu löschen<HelpLink page="faq;archive" /></li>
                        <li><InternalLink text="den Protokollen" view="log" /> von bereits durchgeführten Aufzeichnungen<HelpLink page="faq;log" /></li>
                    </ul>
                    <ul>
                        <li><InternalLink text="den individuellen Anpassungen" view="settings" /> der Web Oberfläche</li>
                    </ul>
                </div>
                <div>
                    Je nach den zugeteilten Benutzerrechten können Sie darüber hinaus folgende administrative Tätigkeiten wahrnehmen:
                    <ul>
                        <li><InternalLink text="die Programmzeitschrift sobald wie möglich aktualisieren" view="" /><HelpLink page="faq;epgconfig" /></li>
                        <li><InternalLink text="einen Sendersuchlauf sobald wie möglich durchführen" view="" /><HelpLink page="faq;psiconfig" /></li>
                        <li>prüfen, ob inzwischen eine <InternalLink text="neuere Version" view="" /> des VCR.NET Recording Service angeboten wird</li>
                        <li><InternalLink text="die Konfiguration des VCR.NET Recording Service verändern" view="admin" /></li>
                    </ul>
                </div>
                <div>
                    Weitere Informationen zum VCR.NET Recording Service findet man hier im Bereich der <InternalLink text="Fragen & Antworten" view="faq;overview" />, auf der <ExternalLink text="Homepage im Internet" url="http://www.psimarron.net/vcrnet" /> oder im <ExternalLink text="offiziellen Forum" url="http://www.watchersnet.de/Default.aspx?tabid=52&g=topics&f=17" />.
                </div>
                <div>
                    Dr. Jochen Manns, 2003-16
                </div>
            </div>;
        }
    }
}
