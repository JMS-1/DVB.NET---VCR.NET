/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class Tasks extends HelpComponent {
        readonly title = "Sonderaufgaben";

        render(page: App.IPage): JSX.Element {
            return <div>
                Neben der Ausführung der vom Anwender programmierten Aufzeichnungen kann der VCR.NET
                Recording Service die DVB Hardware auch für andere Zwecke nutzen.
                <br />
                <br />
                Zum einen sind da die konfigurierbaren periodischen Aufgaben zur Aktualisierung
                von <JMSLib.ReactUi.InternalLink view={`${page.route};epgconfig`}>Programmzeitschrift</JMSLib.ReactUi.InternalLink> und
                der <JMSLib.ReactUi.InternalLink view={`${page.route};psiconfig`}>Liste der verfügbaren Quellen</JMSLib.ReactUi.InternalLink>.
                Bei der Aktualisierung der Programmzeitschrift
                wird für ausgewählte Quellen die von den Sendeanstalten
                angebotene <JMSLib.ReactUi.InternalLink view={`${page.route};epg`}>elektronische Programmzeitschrift</JMSLib.ReactUi.InternalLink> (EPG)
                ausgewertet und für die spätere Programmierung von Aufzeichnungen zusammengeführt. Bei der Aktualisierung der Liste
                der Quellen (Sendersuchlauf) wird für jedes relevante DVB Gerät geprüft,
                welche <JMSLib.ReactUi.InternalLink view={`${page.route};sourcechooser`}>Quellen</JMSLib.ReactUi.InternalLink> grundsätzlich
                empfangen werden können. Je nach Konfiguration im zugehörigen DVB.NET Geräteprofil können auf diese Weise
                auch neu bereitgestellte Quellen ohne weiteres Zutun erkannt werden.
                <br />
                <br />
                Zum anderen ist es möglich, für jedes DVB Gerät den so genannten <em>Live</em> Modus
                zu aktivieren. Ähnlich wie bei einer einzelnen Aufzeichnung wird dabei eine Quelle
                angewählt. Allerdings wird keine Aufzeichnungsdatei erstellt, sondern es werden
                vielmehr die Informationen, die eigentlich in einer solchen abgelegt würden, an
                eine Netzwerkadresse versendet. Diverse frei
                verfügbare <JMSLib.ReactUi.InternalLink view={`${page.route};tsplayer`}>Programme</JMSLib.ReactUi.InternalLink> wie
                VLC können einen so erzeugten Datenstrom empfangen und darstellen. Speziell
                für diese Aufgabe entwickelt wurde
                der <JMSLib.ReactUi.ExternalLink url="http://www.psimarron.net/DVBNETViewer/">DVB.NET / VCR.NET Viewer</JMSLib.ReactUi.ExternalLink>.
                Dieser kann die Informationen nicht nur als Bild und Ton visualisieren, sondern auch den VCR.NET Recording Service
                dazu bewegen, die Quelle nach Wunsch des Anwenders zu ändern. Der Viewer ist damit
                vergleichbar mit einem primitiven Fernseher, der als Empfängereinheit jedes DVB
                Gerät nutzen kann, das auch dem VCR.NET Recording Service zur Verfügung steht.
                <br />
                <br />
                Bei der Planung von Aufzeichnungen haben periodische Aufgaben grundsätzlich eine
                geringere Priorität als programmierte Aufzeichnungen. Würden sich etwa eine Aktualisierung
                und eine Aufzeichnung überschneiden, so wird die Aktualisierung so lange verschoben,
                bis ein ausreichendes Zeitfenster für die Durchführung gefunden wurde. Etwas anders
                sieht es allerdings aus, wenn bereits eine Aktualisierung aktiv ist. Wird im Zeitraum
                der Aktualisierung eine neue Aufzeichnung hinzugefügt und überschneidet sich diese
                mit der laufenden Aktualisierung, so wird die Aufzeichnung verzögert beginnen: eine
                einmal gestartete Aktualisierung wird nicht zwangsweise beendet um einer Aufzeichnung
                Platz zu machen.
                <br />
                <br />
                Der <em>Live</em> Modus kann in der Planung natürlich grundsätzlich nicht berücksichtigt
                werden. Soll eine Aufzeichnung stattfinden, so wird ein aktiver <em>Live</em> Modus
                automatisch beendet. Darüber hinaus ist es sogar nicht möglich, in den <em>Live</em> Modus 
                zu wechseln, wenn eine neue Aufzeichnung in naher Zukunft beginnen
                würde. Gegenüber den Aktualisierungen hat der <em>Live</em> Modus allerdings Vorrang:
                es kann in den <em>Live</em> Modus gewechselt werden, selbst wenn eine Aktualisierung
                unmittelbar bevorsteht. Soll eine Aktualisierung stattfinden, während der <em>Live</em> Modus
                verwendet wird, so wird diese nach hinten verschoben, bis der <em>Live</em> Modus
                beendet wird. Während einer laufenden Aufzeichnung oder Aktualisierung steht
                der <em>Live</em> Modus grundsätzlich nicht zur Verfügung.
                <br />
                <br />
                Anders als bei Aufzeichnungen nutzt der <em>Live</em> Modus ein DVB Gerät immer
                exklusiv. Es ist daher nicht wie
                bei <JMSLib.ReactUi.InternalLink view={`${page.route};parallelrecording`}>parallelen Aufzeichnungen</JMSLib.ReactUi.InternalLink> möglich
                zwei Quellen der gleichen Gruppe im <em>Live</em> Modus an zwei Empfänger zur Anzeige zu schicken.
            </div>;
        }
    }
}
