/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class NumberOfFiles extends HelpComponent {
        readonly title = "Anzahl der Aufzeichnungsdateien pro Aufzeichnung";

        render(page: App.IPage): JSX.Element {
            return <div>
                Im Allgemeinen erzeugt der VCR.NET Recording Service pro Aufzeichnung eine
                einzige <JMSLib.ReactUi.InternalLink view={`${page.route};filecontents`}>Aufzeichnungsdatei</JMSLib.ReactUi.InternalLink> mit
                allen gewünschten Informationen.
                Es gibt allerdings auch Situationen, in denen mehrere Aufzeichnungsdateien angelegt
                werden. Einige Situationen werden bewusst und hoffentlich auch erwünscht auftreten,
                andere dienen der Korrektur von Fehlverhalten.
                <br />
                <br />
                Der wichtigste beabsichtigte Fall mehrerer Aufzeichnungsdateien tritt bei der Aufzeichnung
                von Regionalsendern wie etwa WDR Bonn auf. Eine Aufzeichnung der Quelle WDR Bonn
                wird die meisten Zeit exakt identische Ergebnisse liefern wie die Aufzeichnung von
                WDR Köln oder WDR Aachen. Nur in ganz bestimmten Zeiträumen unterscheiden sich die
                Inhalte der lokalen Varianten des WDR - zum Zeitpunkt der Erstellung dieser Beschreibung
                werden zum Beispiel werktags zwischen 19:30 und 20:00 unter dem Namen <em>Lokalzeit</em>
                regionale Nachrichten aus jeweils eigenen Sendestudios ausgestrahlt.
                <br />
                <br />
                Der VCR.NET Recording Service erkennt diese Umschaltung und erstellt basierend auf
                diesen Zeiten separate Dateien, die neben einem gemeinsamen Namensteil einfach mit
                Hilfe eines Anhängsels durchnummeriert sind. Programmiert man etwa in der Woche
                eine Aufzeichnung auf WDR Bonn von 19:15 bis 20:15, so werden im Allgemeinen drei
                Datei erzeugt: von 19:15 bis 19:30 ein Abschnitt WDR Köln, dann von 19:30 bis 20:00
                die Bonner <em>Lokalzeit</em> und schließlich von 20:00 bis 20:15 die gemeinsamen
                Nachrichten der ARD, die über alle lokalen Varianten identisch empfangen werden.
                Ist man nur an der regionalen Lokalzeit interessiert, so braucht man sich nur um
                die mittlere Aufzeichnungsdatei zu kümmern.
                <br />
                <br />
                Dieses Verhalten ist eine grundsätzlich Eigenschaft des VCR.NET Recording Service
                bei der Aufzeichnung. Ständig wird überwacht, ob sich die Informationen, die in
                die Aufzeichnungsdatei übertragen werden, ursächlich verändern. Im Fall der Lokalfenster
                des WDR werden sowohl Bild als auch Ton umgeschaltet. Eine derartige Trennung würde
                aber auch vorgenommen, wenn einzelne Informationen nachträglich an oder abgeschaltet
                werden. Diese Situation wurde schon lange Zeit nicht mehr beobachtet, trotzdem aber
                hier das Beispiel aus der Vergangenheit: der PayTV Sender PREMIERE hatte die Angewohnheit,
                bei Ausstrahlungen mit Originalton die dafür benötigte Tonspur mit Beginn der Sendung
                anzuschalten und am Ende wieder vollständig zu deaktivieren. Mit dem heutigen Stand
                würde der VCR.NET Recording Service auch hier mehrere Dateien erzeugen - was allerdings
                je nach genauem Vorgehen des Senders durchaus zu unerwünschten Nebeneffekten führen
                kann.
                <br />
                <br />
                Eine Trennung wird zusätzlich auch bei gewissen Fehlersituationen vorgenommen. Je
                nach verwendeter DVB Karte und Installation der Empfangsanlage kann es bei Unwettern
                schon einmal dazu kommen, dass der Empfang nicht nur gestört ist sondern vollständig
                zusammen bricht. Einige DVB Karten stellen dann die Datenübertragung ein und sind
                nicht von sich aus in der Lage, den Normalbetrieb wieder aufzunehmen. Der VCR.NET
                Recording Service erkennt eine solche Situation und versucht dann, die laufenden
                Aufzeichnungen zu beenden und nach einer Re-Initialisierung der Hardware neu zu
                starten. Bei jedem Versuch wird dann eine neue Aufzeichnungsdatei angelegt - werden
                mehrere Versuche dieser Art benötigt, so können auch einige dieser Dateien leer
                sein.
                <br />
                <br />
                Über die <JMSLib.ReactUi.InternalLink view={`${page.route};dvbnet`}>DVB.NET</JMSLib.ReactUi.InternalLink> Geräteprofile
                der verwendeten Hardware
                ist es in begrenztem Maße möglich, das Verhalten bei der Dateitrennung zu beeinflussen.
                <ScreenShot description="Schutzeinstellungen" name="FAQ/streamprotection" />
                Die Einträge haben in dieser Reihenfolge folgende Bedeutung:<br />
                <ul>
                    <li>Das Intervall, in dem geprüft wird, ob noch Daten empfangen werden. Ist dieses zu
                        klein gewählt, so kann fälschlicherweise ein Neustart der Aufzeichnungen erfolgen
                        - dadurch können Aufnahmen unbrauchbar werden.</li>
                    <li>Bisher nicht vorgestellt das Intervall, in dem geprüft wird, ob
                        die <JMSLib.ReactUi.InternalLink view={`${page.route};decryption`}>Entschlüsselung</JMSLib.ReactUi.InternalLink> einer
                        Quelle erfolgreich ist. Einige CI/CAM Kombinationen erfordern
                        ein mehrfaches Ansteuern der Entschlüsselungsschnittstelle bis ein stabiler Empfang
                        gewährleistet ist. Werden in dem angegebenen Intervall keine Daten empfangen, so
                        startet der VCR.NET Recording Service die betroffene Aufzeichnung neu. Ein zu klein
                        gewählter Wert kann bei träger CI/CAM Hardware dazu führen, dass die Initialisierung
                        immer wieder unterbrochen und die Aufnahme unbrauchbar wird.</li>
                    <li>Ein Intervall in dem wie beschrieben geprüft wird, ob sich Informationen in den
                        Aufzeichnungsdateien verändert haben. Ist dieser Wert wie in der Voreinstellung
                        0 so reagiert der VCR.NET Recording Service unmittelbar auf Veränderung wie dem
                        Wechsel in ein Lokalfenster des WDR.</li>
                </ul>
            </div>;
        }
    }
}
