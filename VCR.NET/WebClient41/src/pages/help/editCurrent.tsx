/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class EditCurrent extends HelpComponent {
        readonly title = "Laufende Aufzeichnungen verändern";

        render(page: App.IPage): JSX.Element {
            return <div>
                Sobald eine Aufzeichnung gestartet wird kennt der VCR.NET Recording Service diese
                in zweierlei Weise: da ist zum einen die ursprüngliche Programmierung der Aufzeichnung,
                die im Regelfall unverändert bleibt. Es gibt aber auch eine zweite Sicht über die
                gerade laufende<JMSLib.ReactUi.InternalLink view={page.application.devicesPage.route} pict="devices" /> Aufzeichnung.
                In dieser wird etwa vermerkt, wenn der Anwender den Endzeitpunkt verschiebt.
                Manchmal ist es wichtig zu verstehen, welche Veränderung den VCR.NET Recording Service beeinflussen.
                <br />
                <br />
                Der einfachste Fall ist es, die Programmierung einer Aufzeichnung zu verändern,
                ohne die gerade aktive Ausführung anzurühren. In dieser Situation gilt folgende
                einfache Regel: jede Veränderung wird ignoriert. Der VCR.NET Recording Service wird
                die laufende Aufzeichnung wie ursprünglich programmiert beenden.
                <br />
                <br />
                Wird die laufende Ausführung verändert, so entfernt der VCR.NET Recording Service
                die zugehörige Aufzeichnung aus der
                Planungsansicht<JMSLib.ReactUi.InternalLink view={page.application.planPage.route} pict="plan" />.
                Handelt es sich um
                eine <JMSLib.ReactUi.InternalLink view={`${page.route};repeatingschedules`}>Serienaufzeichnung</JMSLib.ReactUi.InternalLink>,
                so erscheint diese erst bei der nächsten Wiederholung im Plan. Dies kann verwirrend für den Anwender
                sein, stört die Gesamtplanung im VCR.NET Recording Service allerdings nicht.
            </div>;
        }
    }
}
