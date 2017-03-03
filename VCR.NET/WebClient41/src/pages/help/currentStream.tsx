/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class CurrentStream extends HelpComponent {
        readonly title = "Laufende Aufzeichnungen verändern";

        render(page: App.IPage): JSX.Element {
            return <div>
                Die Aktivitäten<JMSLib.ReactUi.InternalLink view={page.application.devicesPage.route} pict="devices" /> auf
                einem DVB Gerät können in einem
                gewissen Rahmen verändert werden. Handelt es sich um
                eine <JMSLib.ReactUi.InternalLink view={`${page.route};tasks`}>Sonderaufgabe</JMSLib.ReactUi.InternalLink> wie
                der Aktualisierung
                der <JMSLib.ReactUi.InternalLink view={`${page.route};epgconfig`}>Programmzeitschrift</JMSLib.ReactUi.InternalLink>,
                so ist das vorzeitige Beenden ebenso möglich wie eine Veränderung der Laufzeit. Grundsätzlich
                wird allerdings empfohlen, Sonderaufgaben nicht in der konfigurierten Ausführung
                zu behindern - maximal kann ein vorzeitiger Abbruch Sinn machen, wenn es äußere
                Umstände erforderlich machen.
                <ScreenShot description="Laufzeit verändern" name="FAQ/endchange" />
                Interessanter ist es, wenn der der VCR.NET Recording Service eine DVB Hardware nutzt
                um Aufzeichnungen auszuführen. Je nach konkreter Situation können dies
                auch <JMSLib.ReactUi.InternalLink view={`${page.route};parallelrecording`}>mehrere Aufzeichnungen</JMSLib.ReactUi.InternalLink> sein,
                die parallel ausgeführt werden, wobei diese allerdings separat in der Aktivitätenliste aufgeführt werden. Hier
                sind nun folgende Veränderungen möglich:
                <br />
                <ul>
                    <li>Eine Aufzeichnung kann vorzeitig beendet werden.</li>
                    <li>Das Ende einer Aufzeichnung kann verschoben werden - eine Verkürzung ist nur soweit
                        möglich, dass der Endzeitpunkt immer noch in der Zukunft liegt.</li>
                </ul>
                Es empfiehlt sich grundsätzlich, Manipulationen an laufenden Aufzeichnungen mit
                Bedacht vorzunehmen. Bei Veränderungen des Endzeitpunktes kann es etwa dazu kommen,
                dass die betroffenen Aufzeichnungen nicht mehr in der Aufzeichnungsplanung
                erscheinen<JMSLib.ReactUi.InternalLink view={page.application.planPage.route} pict="plan" />.
            </div>;
        }
    }
}
