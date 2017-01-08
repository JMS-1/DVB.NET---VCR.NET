/// <reference path="helpComponent.ts" />

namespace VCRNETClient.HelpPages {
    export class Overview extends HelpComponent {
        getTitle(): string {
            return "Fragen und Antworten";
        }

        render(page: App.NoUi.IPage): JSX.Element {
            return <div>
                <p>
                    Wie jede andere Anwendung auch arbeitet der VCR.NET Recording Service nach gewissen
                Prinzipien, die man für einen erfolgreichen Einsatz verstanden haben sollte. Letztlich
                soll VCR.NET ja genau das tun, was man eigentlich will. Auch wenn die Web Oberfläche
                ein intuitives Bedienungskonzept verwendet, so erschließen sich die Funktionalitäten
                sicher nicht immer von selbst. Hier im Hilfebereich, der mit dem VCR.NET Recording
                Service installiert wird und auch ohne Online Verbindung jederzeit zur Verfügung
                steht, sind einige der wesentlichen Aspekte der Bedienung unter Oberbegriffen zusammengefasst.
                Zusätzlich findet man in den einzelnen Bereichen der Anwendung oft das kleine
                Symbol <Pictogram name="info" />, über das direkt zu einer passenden
                Erklärung gesprungen werden kann.
                </p>
                <p>
                    In einigen Fällen führen die im Folgenden aufgeführten Verweise unmittelbar
                zu entsprechenden Seiten der Web Anwendung. Diese bieten dann die Option an, die gewünschten Erläuterungen direkt
                in die Seite einzublenden.
                </p>
                <h3>Aufzeichnungen</h3>
                <ul>
                    <li><InternalLink view={`${page.route};filecontents`}>Was</InternalLink> wird aufgezeichnet?</li>
                    <li>
                        Wann kann es <InternalLink view={`${page.route};numberoffiles`}>mehr als eine</InternalLink> Aufzeichnungsdatei für
                        eine einzige Aufzeichnung geben?
                    </li>
                    <li>Welche Aufzeichnungen können <InternalLink view={`${page.route};parallelrecording`}>gleichzeitig</InternalLink> erfolgen?</li>
                    <li>Warum gibt es <InternalLink view={`${page.route};jobsandschedules`}>Aufträge</InternalLink> und Aufzeichnungen?</li>
                    <li>
                        Wie geht das mit einer <InternalLink view={`${page.route};repeatingschedules`}>Serienaufzeichnung</InternalLink>
                        und den Ausnahmeregeln?
                    </li>
                    <li>Wie wird eine Quelle (ein Sender) für eine Aufzeichnung <InternalLink view={`${page.route};sourcechooser`}>ausgewählt</InternalLink>?</li>
                    <li>
                        Wie wird die <InternalLink view={`${page.route};epg`}>Programmzeitschrift</InternalLink> für die Programmierung von
                        Aufzeichnungen eingesetzt?
                    </li>
                    <li>Wann werden Aufzeichnungen gelöscht und was ist das <InternalLink view={`${page.route};archive`}>Archiv</InternalLink>?</li>
                    <li>
                        Was gibt es bei der Aufzeichnung <InternalLink view={`${page.route};decryption`}>verschlüsselter</InternalLink> Quellen
                        zu beachten?
                    </li>
                    <li>
                        Wie kommt eine Aufzeichnung zu einem <InternalLink view="admin;directories">Dateinamen</InternalLink> für
                        die Aufzeichnungsdatei?
                    </li>
                    <li>Was fängt man mit einer <InternalLink view={`${page.route};tsplayer`}>Aufzeichnungsdatei</InternalLink> an?</li>
                    <li>Wie wird <InternalLink view={`${page.route};customschedule`}>entschieden</InternalLink>, welche Aufzeichnung
                        von welcher DVB Karte ausgeführt wird?
                    </li>
                </ul>
                <h3>Aktivitäten</h3>
                <ul>
                    <li>Wofür wird eine DVB Karte <InternalLink view="current">gerade eingesetzt</InternalLink>?</li>
                    <li>Was sind <InternalLink view={`${page.route};tasks`}>Sonderaufgaben</InternalLink>?</li>
                    <li>
                        Wie können laufende Aufzeichnungen <InternalLink view={`${page.route};currentstream`}>beeinflusst</InternalLink> werden?
                    </li>
                    <li>Was passiert, wenn die Programmierung einer laufenden Aufzeichnung <InternalLink view={`${page.route};editcurrent`}>verändert</InternalLink> wird?</li>
                    <li>Was kann man mit einer laufenden Aufzeichnung alles <InternalLink view={`${page.route};streaming`}>anstellen</InternalLink>?</li>
                    <li>Was enthalten die <InternalLink view={`${page.route};log`}>Aufzeichnungsprotokolle</InternalLink>?</li>
                </ul>
                <h3>Hauppauge Nexus / TechnoTrend 2300</h3>
                <ul>
                    <li>Was ist an dieser DVB Hardware so <InternalLink view={`${page.route};nexus`}>besonders</InternalLink>?</li>
                    <li>
                        Wie kann die Aufzeichnungsplanung auf eine bestimmte Anzahl gleichzeitiger
                        Quellen <InternalLink view={`${page.route};sourcelimit`}>beschränkt</InternalLink> werden?
                    </li>
                </ul>
                <h3>Betriebsumgebung und Konfiguration</h3>
                <ul>
                    <li>Wie ist das mit dem <InternalLink view={`${page.route};hibernation`}>Schlafzustand</InternalLink>?</li>
                    <li>Welche Rolle spielt das <InternalLink view={`${page.route};controlcenter`}>VCR.NET Kontrollzentrum</InternalLink>?</li>
                    <li>Wie konfiguriert man die Aktualisierung der <InternalLink view={`${page.route};epgconfig`}>Programmzeitschrift</InternalLink>?</li>
                    <li>
                        Wie konfiguriert man die Aktualisierung der <InternalLink view={`${page.route};psiconfig`}>Liste der Quellen</InternalLink> (Sendersuchlauf)?
                    </li>
                    <li>
                        Welche sonstigen <InternalLink view={`${page.route};configuration`}>Konfigurationen</InternalLink> bietet der VCR.NET
                        Recording Service an?
                    </li>
                    <li>
                        Welche Rolle spielen <InternalLink view={`${page.route};dvbnet`}>DVB.NET</InternalLink> und die darüber definierten
                        Geräteprofile?
                    </li>
                    <li>
                        Was sind die Web Dienste und
                        welche <InternalLink view={`${page.route};websettings`}>erweiterten Einstellungen</InternalLink> werden angeboten?
                    </li>
                </ul>
            </div>;
        }
    }
}
