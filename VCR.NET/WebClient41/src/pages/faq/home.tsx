/// <reference path="helpComponent.ts" />

namespace VCRNETClient.HelpPages {
    export class Overview extends HelpComponent {
        getTitle(): string {
            return "Fragen und Antworten";
        }

        render(): JSX.Element {
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
                    <li><InternalLink view="faq;filecontents">Was</InternalLink> wird aufgezeichnet?</li>
                    <li>
                        Wann kann es <InternalLink view="faq;numberoffiles">mehr als eine</InternalLink> Aufzeichnungsdatei für
                        eine einzige Aufzeichnung geben?
                    </li>
                    <li>Welche Aufzeichnungen können <InternalLink view="faq;parallelrecording">gleichzeitig</InternalLink> erfolgen?</li>
                    <li>Warum gibt es <InternalLink view="faq;jobsandschedules">Aufträge</InternalLink> und Aufzeichnungen?</li>
                    <li>
                        Wie geht das mit einer <InternalLink view="faq;repeatingschedules">Serienaufzeichnung</InternalLink>
                        und den Ausnahmeregeln?
                    </li>
                    <li>Wie wird eine Quelle (ein Sender) für eine Aufzeichnung <InternalLink view="faq;sourcechooser">ausgewählt</InternalLink>?</li>
                    <li>
                        Wie wird die <InternalLink view="faq;epg">Programmzeitschrift</InternalLink> für die Programmierung von
                        Aufzeichnungen eingesetzt?
                    </li>
                    <li>Wann werden Aufzeichnungen gelöscht und was ist das <InternalLink view="faq;archive">Archiv</InternalLink>?</li>
                    <li>
                        Was gibt es bei der Aufzeichnung <InternalLink view="faq;decryption">verschlüsselter</InternalLink> Quellen
                        zu beachten?
                    </li>
                    <li>
                        Wie kommt eine Aufzeichnung zu einem <InternalLink view="admin;directories">Dateinamen</InternalLink> für
                        die Aufzeichnungsdatei?
                    </li>
                    <li>Was fängt man mit einer <InternalLink view="faq;tsplayer">Aufzeichnungsdatei</InternalLink> an?</li>
                    <li>Wie wird <InternalLink view="faq;customschedule">entschieden</InternalLink>, welche Aufzeichnung
                        von welcher DVB Karte ausgeführt wird?
                    </li>
                </ul>
                <h3>Aktivitäten</h3>
                <ul>
                    <li>Wofür wird eine DVB Karte <InternalLink view="current">gerade eingesetzt</InternalLink>?</li>
                    <li>Was sind <InternalLink view="faq;tasks">Sonderaufgaben</InternalLink>?</li>
                    <li>
                        Wie können laufende Aufzeichnungen <InternalLink view="faq;currentstream">beeinflusst</InternalLink> werden?
                    </li>
                    <li>Was passiert, wenn die Programmierung einer laufenden Aufzeichnung <InternalLink view="faq;editcurrent">verändert</InternalLink> wird?</li>
                    <li>Was kann man mit einer laufenden Aufzeichnung alles <InternalLink view="faq;streaming">anstellen</InternalLink>?</li>
                    <li>Was enthalten die <InternalLink view="faq;log">Aufzeichnungsprotokolle</InternalLink>?</li>
                </ul>
                <h3>Hauppauge Nexus / TechnoTrend 2300</h3>
                <ul>
                    <li>Was ist an dieser DVB Hardware so <InternalLink view="faq;nexus">besonders</InternalLink>?</li>
                    <li>
                        Wie kann die Aufzeichnungsplanung auf eine bestimmte Anzahl gleichzeitiger
                        Quellen <InternalLink view="faq;sourcelimit">beschränkt</InternalLink> werden?
                    </li>
                </ul>
                <h3>Betriebsumgebung und Konfiguration</h3>
                <ul>
                    <li>Wie ist das mit dem <InternalLink view="faq;hibernation">Schlafzustand</InternalLink>?</li>
                    <li>Welche Rolle spielt das <InternalLink view="faq;controlcenter">VCR.NET Kontrollzentrum</InternalLink>?</li>
                    <li>Wie konfiguriert man die Aktualisierung der <InternalLink view="faq;epgconfig">Programmzeitschrift</InternalLink>?</li>
                    <li>
                        Wie konfiguriert man die Aktualisierung der <InternalLink view="faq;psiconfig">Liste der Quellen</InternalLink> (Sendersuchlauf)?
                    </li>
                    <li>
                        Welche sonstigen <InternalLink view="faq;configuration">Konfigurationen</InternalLink> bietet der VCR.NET
                        Recording Service an?
                    </li>
                    <li>
                        Welche Rolle spielen <InternalLink view="faq;dvbnet">DVB.NET</InternalLink> und die darüber definierten
                        Geräteprofile?
                    </li>
                    <li>
                        Was sind die Web Dienste und
                        welche <InternalLink view="faq;websettings">erweiterten Einstellungen</InternalLink> werden angeboten?
                    </li>
                </ul>
            </div>;
        }
    }
}
