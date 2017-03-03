/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class WebSettings extends HelpComponent {
        readonly title = "Web Dienste konfigurieren";

        render(page: App.IPage): JSX.Element {
            return <div>
                Der VCR.NET Recording Service enthält einen Web Server, der über das
                HTTP Protokoll sowohl einen Web Client als auch REST Web Dienste anbietet. Der Web
                Client, in dem diese Erläuterungen in diesem Moment angezeigt werden, wird durch einen Internet
                Browser aufgerufen und erlaubt die interaktive Steuerung und Überwachung des VCR.NET
                Recording Service. Die REST Web Dienste werden von Programmen wie dem VCR.NET Kontrollzentrum
                verwendet, um diese Aufgaben direkt aus einer Anwendung heraus vorzunehmen.
                <br />
                <br />
                Damit der Web Server auf Anfragen reagieren kann, müssen einige
                Einstellungen<JMSLib.ReactUi.InternalLink view={`${page.application.adminPage.route};other`} pict="admin" /> korrekt
                vorgenommen werden. VCR.NET
                wurde ursprünglich für den Betrieb in einem lokalen (LAN) Windows Netzwerk entwickelt.
                Für diese Nutzung muss lediglich der TCP/IP Port für das HTTP Protokoll festgelegt
                werden, üblicherweise wird hier Port 80 verwendet, was auch der Voreinstellung nach
                der Erstinstallation entspricht. Zusätzlich erwartet VCR.NET, dass zur Benutzererkennung
                die in Windows integrierten Sicherheitsprotokolle verwendet werden. Alle mit VCR.NET
                ausgelieferten Werkzeuge wie das Kontrollzentrum nutzen entsprechende Mechanismen.<br />
                <ScreenShot description="Web Server Konfiguration" name="FAQ/webserver" />
                Leider erfordern es die Windows eigenen Sicherheitsprotokolle, dass etwa zwischen
                einem Internet Browser, der den Web Client aufruft, und dem VCR.NET Server nicht
                nur eine Kommunikation über den konfigurierten TCP/IP Port möglich ist sondern dass
                vielmehr weitere elementare TCP/IP Ports zwischen Client und Server freigeschaltet
                werden. Ist dies in einem LAN üblicherweise problemlos möglich, so ist eine Freischaltung
                über Firewalls hinaus eher unüblich. Soll etwa auf den VCR.NET Web Client aus dem
                Internet heraus zugegriffen werden, so ist eine Benutzererkennung auf diesem Wege
                im Allgemeinen nicht möglich.
                <br />
                <br />
                Andere Verfahren zur Benutzererkennung bieten allerdings die Option, ausschließlich
                eine HTTP Port zwischen Client und Server zu nutzen. Das wichtigste ist das Basic
                Protokoll, das von allen Browsern unterstützt wird. Leider hat gerade dieser Standardmechanismus
                die entscheidende Schwäche, dass das Kennwort des Anwenders quasi im Klartext zwischen
                Client und Server ausgetauscht wird. Sind Client und Server nicht identische Rechner,
                so kann dieses Kennwort durch Abhören der Netzwerkkommunikation in falsche Hände
                geraten. Der VCR.NET Recording Service erlaubt trotzdem die Nutzung dieses Protokolls
                ergänzend zur Benutzererkennung über die Windows eigenen Mechanismen, allerdings
                wird von einer alleinigen Aktivierung dieser Möglichkeit abgeraten. Alle VCR.NET
                Werkzeuge nutzen wie erwähnt immer die sichere Benutzererkennung.
                <br />
                <br />
                Aufgrund der mangelnden Sicherheit wird eine Benutzererkennung über das Basic Protokoll
                oft an eine verschlüsselte HTTP Verbindung gebunden. Hier erfolgt der Aufruf des
                Web Clients aus dem Internet Browser heraus über das HTTPS Präfix, wodurch dann
                die Kommunikation zwischen Client und Server mittels SSL verschlüsselt wird. Der
                VCR.NET Recording Service erlaubt es, zusätzlich zur regulären Konfiguration über
                HTTP auch diesen Weg freizuschalten. In diesem Fall muss ein weiterer TCP/IP Port
                festgelegt werden, auf den der Web Server im VCR.NET Recording Service bei verschlüsselten
                Verbindungen reagieren soll. Voreigestellt ist hier als Standard der Port 443.
                <br />
                <br />
                Die Konfiguration des VCR.NET Recording Service bietet allerdings weder die Möglichkeit,
                ein SSL Zertifikat zu erstellen noch ein existierendes zur Verschlüsselung der
                Kommunikation zu nutzen. Diese Zuordnung muss einmalig gesondert manuell vorgenommen
                werden - unter Windows 7 übernimmt der <em>netsh http add sslcert</em> den entscheidenden
                Schritt, eine SSL Zertifikat auf dem in der VCR.NET Konfiguration festgelegten TCP/IP
                Port zu nutzen. Erst nach dieser Zuordnung kann VCR.NET die Zugriffe auf die Web
                Dienste wie gewünscht verschlüsseln.
                <br />
                <br />
                Wie auch bei der Benutzererkennung ignorieren alle VCR.NET Werkzeuge diese Einstellungen
                und können daher nur im lokalen Windows Netzwerk eingesetzt werden. Das gilt vor
                allem auch für
                den <JMSLib.ReactUi.ExternalLink url="http://www.psimarron.net/DVBNETViewer/">VCR.NET / DVB.NET Viewer</JMSLib.ReactUi.ExternalLink>:
                das Betrachten von laufenden Aufzeichnungen etwa
                ist über einen Internet Zugang grundsätzlich nicht möglich.
            </div>;
        }
    }
}
