/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class SourceLimit extends HelpComponent {
        readonly title = "Beschränkung der Anzahl der Quellen";

        render(page: App.IPage): JSX.Element {
            return <div>
                Die Planung des VCR.NET Recording Service erwartet, dass im Normalfall auf jedem
                DVB Gerät beliebige viele Aufzeichnungen einer
                Quellgruppe <JMSLib.ReactUi.InternalLink view={`${page.route};parallelrecording`}>gleichzeitig</JMSLib.ReactUi.InternalLink> ausgeführt
                werden können. In gewissen Situation ist dies allerdings
                nicht empfehlenswert oder
                bei <JMSLib.ReactUi.InternalLink view={`${page.route};nexus`}>bestimmter DVB Hardware</JMSLib.ReactUi.InternalLink> auch
                gar nicht möglich.
                <br />
                <br />
                Über die Aufzeichnungseinstellung im zugehörigen DVB.NET Geräteprofil ist es möglich,
                die Anzahl der gleichzeitig verwendeten Quellen zu beschränken. Der VCR.NET Recording
                Service wird dann davon ausgehen, dass nur die angegeben Anzahl unterschiedlicher
                Quellen einer Quellgruppe gleichzeitig verwendet werden dürfen - mehrere Aufzeichnung
                auf ein und derselben Quelle sind von dieser Einschränkung natürlich nicht betroffen.
                <ScreenShot description="Quellen beschränken" name="FAQ/sourcelimit" />
                Im Allgemeinen macht es aber wenig Sinn, diesen Wert von der Voreinstellung (15)
                weg zu verändern. Vor allem unterscheidet der VCR.NET Recording Service nicht zwischen
                Fernseh- und Radiosendern. Selbst wenn eine Aufzeichnung von zwei bis drei Fernsehsendern
                an die Grenzen einer DVB Hardware stößt kann es trotzdem sein, dass 8 Radiosender
                ohne Probleme gleichzeitig aufgezeichnet werden können. Den Wert höher als die Voreinstellung
                zu setzen sollte auch in den wenigsten Situationen notwendig sein.
                <br />
                <br />
                Insgesamt kann nur empfohlen werden, die Einstellung nicht zu verändern und den
                gesunden Menschenverstand auch bei der Programmierung von Aufzeichnungen sinnvoll
                zu nutzen.
            </div>;
        }
    }
}
