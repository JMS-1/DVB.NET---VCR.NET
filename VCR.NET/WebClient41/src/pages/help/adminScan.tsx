/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class AdminSourceScan extends HelpComponent {
        readonly title = "Quellen aktualisieren (Sendersuchlauf)";

        render(page: App.IPage): JSX.Element {
            return <div>
                Zur Programmierung von Aufzeichnungen bietet der VCR.NET Recording Service für
                jedes DVB Gerät die <JMSLib.ReactUi.InternalLink view={`${page.route};sourcechooser`}>Quellen
                (Sender)</JMSLib.ReactUi.InternalLink> an, die im
                zugehörigen <JMSLib.ReactUi.InternalLink view={`${page.route};dvbnet`}>DVB.NET</JMSLib.ReactUi.InternalLink> Geräteprofil
                hinterlegt sind. Diese Liste ändert sich je nach Empfangsart von Zeit zu Zeit und
                es macht dann Sinn, eine Aktualisierung durchzuführen. Das kann manuell mit dem <em>DVB.NET
                Konfigurations- und Administrationswerkzeug</em> geschehen - in diesem Fall wird
                empfohlen, den VCR.NET Recording Service für den Zeitraum der Aktualisierung zu beenden.
                <br />
                <br />
                Alternativ ist es aber auch möglich, dem VCR.NET Recording Service dafür einzurichten,
                für alle DVB Geräte eine entsprechende
                Aktualisierung<JMSLib.ReactUi.InternalLink view={`${page.application.adminPage.route};sources`} pict="admin" /> periodisch
                auszuführen.
                <ScreenShot description="Aktualisierung konfigurieren" name="FAQ/psi" />
                Im Endeffekt erfolgt die Konfiguration ganz analog
                zur <JMSLib.ReactUi.InternalLink view={`${page.route};epgconfig`}>Programmzeitschrift</JMSLib.ReactUi.InternalLink>.
                Die Aktualisierung kann gänzlich deaktiviert werden, nur manuell erfolgen oder wie
                im Bild gezeigt einem Zeitplan folgen. Zuerst einmal erfolgt die Angabe der vollen
                Stunden, an denen eine Aktualisierung stattfinden darf - je nach Planung der Aufzeichnungen
                kann dieser Zeitpunkt auch nach hinten geschoben werden,
                da <JMSLib.ReactUi.InternalLink view={`${page.route};tasks`}>Sonderaufgaben</JMSLib.ReactUi.InternalLink> wie
                die Aktualisierung der Liste der Quellen eine niedrigere Priorität als reguläre
                Aufzeichnungen haben. Es macht hier durchaus Sinn, mindestens zwei Zeiten zu verwenden,
                wobei die eine im Nachtbereich und die andere tagsüber liegt, da es eine ganze Reihe von
                Quellen gibt, die nur zeitweise verfügbar sind.
                <br />
                <br />
                Zusätzlich wird festgelegt, wie viel Zeit zwischen zwei Aktualisierungen verstreichen
                soll. Im Allgemeinen sollte eine Aktualisierung alle ein bis vier Wochen reichen
                - es ist auch bei einer periodischen Aktualisierung bei Bedarf jederzeit möglich,
                eine manuelle Aktualisierung anzufordern.
                <br />
                <br />
                Die Angabe der maximalen Laufzeit erlaubt es dem VCR.NET Recording Service die Aktualisierung
                im Aufzeichnungsplan zu berücksichtigen. Ist dieser Wert zu klein gewählt, so wird
                die Aktualisierung vorzeitig beendet und es werden eventuell nicht alle Neuerungen
                übernommen. Diesbezüglich ist ein zu großer Wert kein Problem, da die Aktualisierung
                dann einfach zu früh beendet wird. Es wird aber empfohlen, immer einen sinnvollen
                Erfahrungswert zu verwenden, der knapp über dem tatsächlichen Bedürfnis liegt -
                dieser Wert kann
                den <JMSLib.ReactUi.InternalLink view={page.application.logPage.route}>Protokollen</JMSLib.ReactUi.InternalLink> entnommen
                werden.
                <br />
                <br />
                Normalerweise wird die neue Liste mit der vorherigen zusammen geführt. Das ist gerade
                für Quellen wichtig, die nicht ständig verfügbar, da ansonsten nur die Quellen für
                eine Programmierung verwendet werden können, die gerade zum Zeitpunkt der Aktualisierung
                aktiv waren. Manchmal allerdings haben sich in der Liste der Quellen so viele Leichen
                nicht mehr existierender oder verschobener Quellen angesammelt, dass sich eine vollständige
                Bereinigung lohnt. In diesem Fall sollte die Markierung der Option entfernt, eine
                neue Aktualisierung manuell gestartet und danach die Markierung wieder hergestellt
                werden.
                <br />
                <br />
                Genau wie bei der Programmzeitschrift gibt es auch bei der Aktualisierung der Liste
                der Quellen einen optionalen Wert für die Latenzzeit, hier allerdings in Tagen.
                Diese Einstellung erlaubt es dem VCR.NET Recording Service eine Aktualisierung vorzuziehen,
                wenn gerade ein Übergang in den Schlafzustand bevor steht und seit der vorherige Aktualisierung
                das konfigurierte Zeitintervall bereits überschritten wurde.
                <br />
            </div>;
        }
    }
}
