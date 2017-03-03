/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class TsPlayer extends HelpComponent {
        readonly title = "Aufzeichnungsdateien verarbeiten";

        render(page: App.IPage): JSX.Element {
            return <div>
                Der VCR.NET Recording Service
                erstellt <JMSLib.ReactUi.InternalLink view={`${page.route};filecontents`}>Aufzeichnungsdateien</JMSLib.ReactUi.InternalLink> im
                TS (<em>Transport Stream</em>) Format. Obwohl es sich nicht um ein Standardformat
                von Windows handelt, kann es von den meisten Media Playern direkt angezeigt werden
                - in einigen Fällen sogar von der Windows eigenen Anwendung. Erwähnenswert ist an
                dieser Stelle vor allem der
                kostenlose <JMSLib.ReactUi.ExternalLink url="http://www.videolan.org/">VLC</JMSLib.ReactUi.ExternalLink> Media
                Player, mit dem auch ergänzende Informationen wie der Videotext
                angezeigt werden können - natürlich nur wenn ein solcher auch aufgezeichnet wurde.
                <br />
                <br />
                Für die Nachbearbeitung um etwa eine Aufzeichnung auf einer DVD zu verewigen erwarten
                viele Autorenwerkzeuge keine einzelne Datei sondern vielmehr separate Dateien für
                die einzelnen Inhalte wie Bild oder Ton. Manche Werkzeuge haben eine solche Zerlegung
                fest integriert, kostenlose Anwendungen wie die Kombination
                aus <JMSLib.ReactUi.ExternalLink url="http://www.cuttermaran.de/">Cuttermaran</JMSLib.ReactUi.ExternalLink> und <JMSLib.ReactUi.ExternalLink url="http://www.boraxsoft.de/">GfD</JMSLib.ReactUi.ExternalLink> hingegen
                erwarten, dass diese im Vorfeld geschieht. Auch hier gibt
                es kostenlose Alternativen
                wie <JMSLib.ReactUi.ExternalLink url="http://forum.dvbtechnics.info/forumdisplay.php?f=16">ProjectX</JMSLib.ReactUi.ExternalLink> oder <JMSLib.ReactUi.ExternalLink url="http://www.offeryn.de/dv.htm">PVStrumento</JMSLib.ReactUi.ExternalLink> die
                nicht nur eine solche Zerlegung vornehmen, sondern
                dabei auch gleichzeitig den Inhalt der Aufzeichnungsdatei auf Fehler prüfen und
                einfache Korrekturen vornehmen.
            </div>;
        }
    }
}
