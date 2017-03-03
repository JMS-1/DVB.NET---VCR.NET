/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class FileContents extends HelpComponent {
        readonly title = "Inhalt einer Aufzeichnungsdatei";

        render(page: App.IPage): JSX.Element {
            return <div>
                Für jede programmierte Aufzeichnung erstellt der VCR.NET Recording Service eine
                Datei mit der Endung TS (<em>Transport Stream</em>). Abhängig von der Auswahl der
                Quelle (des Senders) und den bei der Programmierung verwendeten Optionen können
                in einer solchen Datei folgende Informationen enthalten sein:
                <ul>
                    <li>Für Fernsehsender ist immer ein Bildsignal vorhanden, für Radiosender natürlich
                        nie. Je nach programmiertem Sender kann es sich um ein digitales Signal einfacher
                        Auflösung (SDTV, MPEG-2) oder hoher Auflösung (HDTV, H.264) handeln.</li>
                    <li>Jeder Sender bietet immer eine primäre Tonspur an, üblicherweise als Stereosignal.
                        Der VCR.NET Recording Service zeichnet dieses Signal immer mit auf.</li>
                    <li>Optional können weitere Stereotonspuren angeboten sein, etwa alternative Sprachen
                        (wie bei ARTE üblich) oder mit eingemischtem Begleittext (wie von ARD und ZDF vereinzelt
                        angeboten). Der VCR.NET Recording Service kann durch die Programmierung einer Aufzeichnung
                        aufgefordert werden, alle diese Stereosignale mit aufzuzeichnen. Eine Einzelauswahl
                        ist nicht möglich.</li>
                    <li>Je nach Sender werden auch höherwertige Tonspuren im Dolby Digital Format angeboten.
                        Bei der Programmierung ist es möglich, diese mit aufzuzeichnen. Entweder nur das
                        primäre Signal oder alle Dolby Digital Spuren - in letzterem Fall aber nur in Kombination
                        mit allen alternativen Stereosignalen.</li>
                    <li>Wenn ein Videotext gesendet wird, kann dieser als Ganzes in die Aufzeichnungsdatei
                        integriert werden. Eine Beschränkung auf einzelne Seiten ist nicht vorgesehen.</li>
                    <li>Viele Sender bieten für ausgewählte Sendungen Untertitel über den Videotext an -
                        üblich sind hier in Deutschland die Seiten 150, 777 und 149, je nach Sender. Zusätzlich
                        werden inzwischen aber auch vielfach digitale Untertitel unabhängig vom Videotext
                        ausgestrahlt. Auch hierfür bietet die Programmierung im VCR.NET Recording Service
                        eine Option zur Aufzeichnung an.</li>
                    <li>Im Allgemeinen wird in jede Aufzeichnungsdatei ein Auszug aus der Programmzeitschrift
                        eingebettet, in dem ausschließlich die aufgezeichneten Sendungen beschrieben sind
                        - sofern die Quelle eine elektronische Programmzeitschrift anbietet, was aber in
                        der Praxis immer der Fall ist. In einzelnen Fällen kann es notwendig sein, diese
                        Integration zu deaktivieren, was aber nur über die Konfiguration
                        des <JMSLib.ReactUi.InternalLink view={`${page.route};dvbnet`}>DVB.NET Geräteprofils</JMSLib.ReactUi.InternalLink> möglich
                        ist.</li>
                </ul>
                Am Rande sei bemerkt, dass auch viele Radiosender alternative Tonspuren anbieten.
                Je nach aufgezeichneter Sendung kann zum Beispiel die höherwertige Dolby Digital
                Spur von Interesse sein.
                <br />
                <br />
                Bei der Programmierung einer Aufzeichnung kann nach
                der <JMSLib.ReactUi.InternalLink view={`${page.route};sourcechooser`}>Auswahl</JMSLib.ReactUi.InternalLink> einer 
                Quelle festgelegt werden, welche der aufgeführten optionalen
                Informationen in die Aufzeichnungsdatei gelangen sollen. Ist eine technische Bereitstellung
                der gewünschten Informationen möglich, so erfüllt der VCR.NET Recording Server die
                mit der Programmierung geäußerten Wünsche, ansonsten werden die Einstellungen kommentarlos
                ignoriert - etwa wenn ein Videotext aufgezeichnet werden soll, die Quelle aber keinen
                anbietet weil es sich um einen Radiosender handelt.<br />
                <ScreenShot description="Optionen festlegen" name="FAQ/streamoptions" />
                Die einzelnen Auswahlpunkte haben dabei folgende Bedeutung (von links nach rechts):
                <br />
                <ul>
                    <li>Primäre Dolby Digital Tonspur aufzeichnen.</li>
                    <li>Alle Stereotonspuren aufzeichnen - inklusive aller Dolby Digital Spuren, wenn auch
                        die vorherige Option gewählt wurde.</li>
                    <li>Videotext in die Aufzeichnungsdatei mit aufnehmen.</li>
                    <li>Alle digitalen Untertitel zusätzlich aufzeichnen.</li>
                </ul>
            </div>;
        }
    }
}
