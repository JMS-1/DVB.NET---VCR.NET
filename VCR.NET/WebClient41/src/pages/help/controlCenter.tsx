/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class ControlCenter extends HelpComponent {
        readonly title = "VCR.NET Kontrollzentrum";

        render(page: App.IPage): JSX.Element {
            return <div>
                Der VCR.NET Recording Service ist ein völlig eigenständiger Windows Dienst, der
                über seine Web Oberfläche vollständig bedient werden kann. Eine Installation bringt
                aber auch zusätzlich eine kleine Anwendung mit, die <em>VCR.NET Kontrollzentrum</em>
                genannt wird. Sie wird üblicherweise automatisch beim Anmelden eines Benutzers gestartet
                und erscheint als kleines Kamerasymbol im System Tray von Windows. Normalerweise
                ist das Kontrollzentrum mit dem lokalen VCR.NET Recording Service verbunden, allerdings
                kann diese Konfiguration verändert und auch eine Verbindung mit einem Dienst auf
                einem anderen Rechner hergestellt werden. Es ist sogar möglich, die Anwendung gleichzeitig
                mit mehreren Rechner zu verbinden, auf denen ein VCR.NET Recording Service aktiv
                ist - in diesem Fall erscheinen dann mehrere Kamerasymbole, deren Tooltips den zugehörigen
                Rechner anzeigen.
                <br />
                <br />
                Schon das Kamerasymbol hat einen gewissen Nutzen für den Anwender: seine Hinterlegung
                mit einer Farbe zeigt an, in welchem Zustand sich der zugehörige VCR.NET Recording
                Service befindet. Eine <span className="vccRed">rote</span> Hinterlegung bedeutet, dass
                keine Verbindung hergestellt werden konnte - bei einem lokalen VCR.NET Recording
                Service kann es sein, dass der zugehörige Windows Dienst gerade nicht aktiv ist
                und bei einem entfernten System kann es zusätzlich an der Konfiguration der Firewalls
                liegen. Ist eine <span className="vccBlue">blaue</span> Hinterlegung zu sehen, so führt
                der VCR.NET Recording Service mindestens eine
                Aktivität<JMSLib.ReactUi.InternalLink view={page.application.devicesPage.route} pict="devices" /> aus
                und es empfiehlt sich, den zugehörigen Rechner nicht neu zu starten oder in
                den <JMSLib.ReactUi.InternalLink view={`${page.route};hibernation`}>Schlafzustand</JMSLib.ReactUi.InternalLink> zu 
                versetzen. Das gilt auch für eine <span className="vccYellow">gelbe</span> Hinterlegung, bei der zwar kein DVB Gerät
                in Benutzung ist, eine Aktivität aber kurz bevor steht. Im Normalzustand ist das
                Kamerasymbol <span className="vccGreen">Grün</span> hinterlegt und der VCR.NET Recording
                Service wartet auf die nächste anstehende Aufzeichnung - fehlt die grüne Hinterlegung,
                so ist der Aufzeichnungsplan<JMSLib.ReactUi.InternalLink view={page.application.planPage.route} pict="plan" /> leer.
                <br />
                <br />
                Hier nun einige weitere Funktionalitäten des Kontrollzentrums - in einigen Fällen
                wird eine korrekte Konfiguration der Anwendung vorausgesetzt, auf die aber nicht
                weiter eingegangen wird:<ul>
                    <li>Aufruf einiger ausgewählter Seiten der Web Oberfläche des zugehörigen VCR.NET Recording
                        Service.</li>
                    <li>Aktivierung des <JMSLib.ReactUi.InternalLink view={`${page.route};streaming`}><em>Live</em> Modus</JMSLib.ReactUi.InternalLink> auf 
                        einem gerade nicht benutzten DVB Gerät - das macht nur bei lokal
                        installiertem <JMSLib.ReactUi.ExternalLink url="http://www.psimarron.net/DVBNETViewer/">DVB.NET / VCR.NET Viewer</JMSLib.ReactUi.ExternalLink> Sinn.</li>
                    <li>Betrachten einer laufenden Aufzeichnung.</li>
                    <li>Unterstützung des Übergangs in den <JMSLib.ReactUi.InternalLink view={`${page.route};hibernation`}>Schlafzustand</JMSLib.ReactUi.InternalLink>.</li>
                </ul>
            </div>;
        }
    }
}
