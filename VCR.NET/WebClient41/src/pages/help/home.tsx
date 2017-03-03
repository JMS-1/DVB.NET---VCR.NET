/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class Overview extends HelpComponent {
        readonly title = "Fragen und Antworten";

        render(page: App.IPage): JSX.Element {
            return <div>
                Wie jede andere Anwendung auch arbeitet der VCR.NET Recording Service nach gewissen
                Prinzipien, die man für einen erfolgreichen Einsatz verstanden haben sollte. Letztlich
                soll VCR.NET ja genau das tun, was man eigentlich will. Auch wenn die Web Oberfläche
                ein intuitives Bedienungskonzept verwendet, so erschließen sich die Funktionalitäten
                sicher nicht immer von selbst. Hier im Hilfebereich, der mit dem VCR.NET Recording
                Service installiert wird und auch ohne Online Verbindung jederzeit zur Verfügung
                steht, sind einige der wesentlichen Aspekte der Bedienung unter Oberbegriffen zusammengefasst.
                Zusätzlich findet man in den einzelnen Bereichen der Anwendung oft das kleine
                Symbol<JMSLib.ReactUi.Pictogram name="info" />, über das direkt zu einer passenden
                Erklärung gesprungen werden kann.
                <br />
                <br />
                In einigen Fällen führen die im Folgenden aufgeführten Verweise unmittelbar
                zu entsprechenden Seiten der Web Anwendung. Diese bieten dann die Option an, die gewünschten Erläuterungen direkt
                in die Seite einzublenden.
                <h3>Aufzeichnungen</h3>
                <ul>
                    <li><JMSLib.ReactUi.InternalLink view={`${page.route};filecontents`}>Was</JMSLib.ReactUi.InternalLink> wird aufgezeichnet?</li>
                    <li>Wann kann es <JMSLib.ReactUi.InternalLink view={`${page.route};numberoffiles`}>mehr als eine</JMSLib.ReactUi.InternalLink> Aufzeichnungsdatei für eine einzige Aufzeichnung geben?</li>
                    <li>Welche Aufzeichnungen können <JMSLib.ReactUi.InternalLink view={`${page.route};parallelrecording`}>gleichzeitig</JMSLib.ReactUi.InternalLink> erfolgen?</li>
                    <li>Warum gibt es <JMSLib.ReactUi.InternalLink view={`${page.route};jobsandschedules`}>Aufträge</JMSLib.ReactUi.InternalLink> und Aufzeichnungen?</li>
                    <li>Wie geht das mit einer <JMSLib.ReactUi.InternalLink view={`${page.route};repeatingschedules`}>Serienaufzeichnung</JMSLib.ReactUi.InternalLink> und den Ausnahmeregeln?</li>
                    <li>Wie wird eine Quelle (ein Sender) für eine Aufzeichnung <JMSLib.ReactUi.InternalLink view={`${page.route};sourcechooser`}>ausgewählt</JMSLib.ReactUi.InternalLink>?</li>
                    <li>Wie wird die <JMSLib.ReactUi.InternalLink view={`${page.route};epg`}>Programmzeitschrift</JMSLib.ReactUi.InternalLink> für die Programmierung von Aufzeichnungen eingesetzt?</li>
                    <li>Wann werden Aufzeichnungen gelöscht und was ist das <JMSLib.ReactUi.InternalLink view={`${page.route};archive`}>Archiv</JMSLib.ReactUi.InternalLink>?</li>
                    <li>Was gibt es bei der Aufzeichnung <JMSLib.ReactUi.InternalLink view={`${page.route};decryption`}>verschlüsselter</JMSLib.ReactUi.InternalLink> Quellen zu beachten?</li>
                    <li>Wie kommt eine Aufzeichnung zu einem <JMSLib.ReactUi.InternalLink view={`${page.application.adminPage.route};directories`}>Dateinamen</JMSLib.ReactUi.InternalLink> für die Aufzeichnungsdatei?</li>
                    <li>Was fängt man mit einer <JMSLib.ReactUi.InternalLink view={`${page.route};tsplayer`}>Aufzeichnungsdatei</JMSLib.ReactUi.InternalLink> an?</li>
                    <li>Wie wird <JMSLib.ReactUi.InternalLink view={`${page.route};customschedule`}>entschieden</JMSLib.ReactUi.InternalLink>, welche Aufzeichnung von welcher DVB Karte ausgeführt wird?</li>
                </ul>
                <h3>Aktivitäten</h3>
                <ul>
                    <li>Wofür wird eine DVB Karte <JMSLib.ReactUi.InternalLink view={page.application.devicesPage.route}>gerade eingesetzt</JMSLib.ReactUi.InternalLink>?</li>
                    <li>Was sind <JMSLib.ReactUi.InternalLink view={`${page.route};tasks`}>Sonderaufgaben</JMSLib.ReactUi.InternalLink>?</li>
                    <li>Wie können laufende Aufzeichnungen <JMSLib.ReactUi.InternalLink view={`${page.route};currentstream`}>beeinflusst</JMSLib.ReactUi.InternalLink> werden?</li>
                    <li>Was passiert, wenn die Programmierung einer laufenden Aufzeichnung <JMSLib.ReactUi.InternalLink view={`${page.route};editcurrent`}>verändert</JMSLib.ReactUi.InternalLink> wird?</li>
                    <li>Was kann man mit einer laufenden Aufzeichnung alles <JMSLib.ReactUi.InternalLink view={`${page.route};streaming`}>anstellen</JMSLib.ReactUi.InternalLink>?</li>
                    <li>Was enthalten die <JMSLib.ReactUi.InternalLink view={`${page.route};log`}>Aufzeichnungsprotokolle</JMSLib.ReactUi.InternalLink>?</li>
                </ul>
                <h3>Hauppauge Nexus / TechnoTrend 2300</h3>
                <ul>
                    <li>Was ist an dieser DVB Hardware so <JMSLib.ReactUi.InternalLink view={`${page.route};nexus`}>besonders</JMSLib.ReactUi.InternalLink>?</li>
                    <li>Wie kann die Aufzeichnungsplanung auf eine bestimmte Anzahl gleichzeitiger Quellen <JMSLib.ReactUi.InternalLink view={`${page.route};sourcelimit`}>beschränkt</JMSLib.ReactUi.InternalLink> werden?</li>
                </ul>
                <h3>Betriebsumgebung und Konfiguration</h3>
                <ul>
                    <li>Wie ist das mit dem <JMSLib.ReactUi.InternalLink view={`${page.route};hibernation`}>Schlafzustand</JMSLib.ReactUi.InternalLink>?</li>
                    <li>Welche Rolle spielt das <JMSLib.ReactUi.InternalLink view={`${page.route};controlcenter`}>VCR.NET Kontrollzentrum</JMSLib.ReactUi.InternalLink>?</li>
                    <li>Wie konfiguriert man die Aktualisierung der <JMSLib.ReactUi.InternalLink view={`${page.route};epgconfig`}>Programmzeitschrift</JMSLib.ReactUi.InternalLink>?</li>
                    <li>Wie konfiguriert man die Aktualisierung der <JMSLib.ReactUi.InternalLink view={`${page.route};psiconfig`}>Liste der Quellen</JMSLib.ReactUi.InternalLink> (Sendersuchlauf)?</li>
                    <li>Welche sonstigen <JMSLib.ReactUi.InternalLink view={`${page.route};configuration`}>Konfigurationen</JMSLib.ReactUi.InternalLink> bietet der VCR.NET Recording Service an?</li>
                    <li>Welche Rolle spielen <JMSLib.ReactUi.InternalLink view={`${page.route};dvbnet`}>DVB.NET</JMSLib.ReactUi.InternalLink> und die darüber definierten Geräteprofile?</li>
                    <li>Was sind die Web Dienste und welche <JMSLib.ReactUi.InternalLink view={`${page.route};websettings`}>erweiterten Einstellungen</JMSLib.ReactUi.InternalLink> werden angeboten?</li>
                </ul>
            </div>;
        }
    }
}
