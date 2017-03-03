/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class JobsAndSchedules extends HelpComponent {
        readonly title = "Aufträge und Aufzeichnungen";

        render(page: App.IPage): JSX.Element {
            return <div>
                Schaut man sich die Programmierung von Aufzeichnungen etwas genauer an, dann sieht
                man, dass der VCR.NET Recording Service hier auf zwei Konzepten aufsetzt.
                <ScreenShot description="Auftrag und Aufzeichnung" name="FAQ/jobsandschedules" />
                Eine Aufzeichnung wird grundsätzlich als Teil eines Auftrags angelegt. Über die
                Liste aller Aufzeichnungen<JMSLib.ReactUi.InternalLink view={page.application.jobPage.route} pict="jobs" /> (nicht den
                Aufzeichnungsplan<JMSLib.ReactUi.InternalLink view={page.application.planPage.route} pict="plan" />) ist
                es dann auch möglich, einem Auftrag mehrere Aufzeichnungen zuzuordnen. Einige der
                für eine Aufzeichnung notwendigen Parameter sind nur über den Auftrag verfügbar
                und werden von allen Aufzeichnungen gemeinsam genutzt.
                <br />
                <br />
                Der Hintergrund dieser Trennung liegt in der Historie des VCR.NET Recording Service
                begründet. In der Entstehungszeit im Jahre 2003 wurde der VCR.NET hauptsächlich
                dafür eingesetzt <JMSLib.ReactUi.InternalLink view={`${page.route};repeatingschedules`}>Serien</JMSLib.ReactUi.InternalLink> aufzuzeichnen.
                Zwar bietet die Serienaufnahme einer einzelnen Aufzeichnung die Möglichkeit, diese an
                verschiedenen Wochentagen zu wiederholen, aber das war es noch nicht ganz. Gerade
                bei Serien, die auch am Wochenende ausgestrahlt werden ergeben sich oft deutlich
                andere Sendezeiten. Manchmal lassen diese sich durch großzügige Vor- und Nachlaufzeiten
                kompensieren, manchmal ufert dieser Ansatz aber auch einfach aus. Daher hat der
                VCR.NET Recording Service die Möglichkeit erhalten, mehrere Aufzeichnungen in einem
                Auftrag zu bündeln.
                <br />
                <br />
                Für alle Aufzeichnungen ist insbesondere die Auswahl des verwendeten DVB Gerätes
                und des <JMSLib.ReactUi.InternalLink view={`${page.route};configuration`}>Ablageverzeichnisses</JMSLib.ReactUi.InternalLink> für
                die Aufzeichnungsdateien gemeinsam. Auf Wunsch kann sogar die Quelle (der Sender) vorgewählt werden und muss
                dann nicht mehr pro Aufzeichnung explizit angegeben werden - es ist aber möglich,
                die Quelle individuell für jede Aufzeichnung festzulegen. Ähnlich verhält es sich
                mit dem Namen: ein Auftrag hat immer einen Namen, der im Allgemeinen ein wesentlicher
                Bestandteil des Dateinamens von Aufzeichnungsdateien
                ist. Eine Aufzeichnung muss keinen eigenen Namen haben. Wird allerdings ein solcher
                angegeben, so trägt er üblicherweise zusätzlich zum Namen des Auftrags zum Dateinamen
                bei.
                <br />
                <br />
                Es ist möglich, einzelne Aufzeichnungen eines Auftrags zu löschen. Diese werden
                dann aus dem Auftrag entfernt, alle weiteren Aufzeichnungen bleiben unberührt. Beim
                Entfernen der letzten Aufzeichnung wird
                der <JMSLib.ReactUi.InternalLink view={`${page.route};archive`}>Auftrag automatisch mit entfernt</JMSLib.ReactUi.InternalLink> - das
                ist vermutlich der Normalfall bei Einsatz des VCR.NET
                Recording Service. Ergänzend ist es auch möglich, einen Auftrag als Ganzes samt
                aller Aufzeichnungen drin in einem Rutsch zu entfernen.
            </div>;
        }
    }
}
