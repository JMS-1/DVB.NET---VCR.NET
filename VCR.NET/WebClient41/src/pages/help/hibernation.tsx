/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class Hibernation extends HelpComponent {
        readonly title = "Schlafzustand";

        render(page: App.IPage): JSX.Element {
            return <div>
                Der VCR.NET Recording Service ist ein unsichtbar im Hintergrund laufender Windows
                Dienst, der selbstständig programmierte Aufzeichnungen
                und <JMSLib.ReactUi.InternalLink view={`${page.route};tasks`}>Sonderaufgaben</JMSLib.ReactUi.InternalLink> ausführen
                soll. Dabei ist die Zusammenarbeit mit dem Schlafzustand des Rechners
                ein wichtiger Aspekt. Zum Verständnis der Abläufen werden nun einige Szenarien vorgestellt,
                beginnend mit einem eher untypischen Fall: dem Betrieb des VCR.NET auf einem gesonderten
                Rechner, dessen einzige Aufgabe die Aufzeichnung von DVB Ausstrahlungen ist. Der
                untypische Fall für dieses Beispiel ist, dass es dem VCR.NET auch gestattet sei,
                den Rechner in den Schlafzustand zu versetzen - ohne zusätzliche Maßnahmen steht
                der Rechner dann nicht ständig zur Programmierung neuer Aufzeichnungen zur Verfügung.
                <ScreenShot description="Einstellungen zum Schlafzustand" name="FAQ/hibernate" />
                Grundsätzlich stellt der VCR.NET Recording Service sicher, dass der Rechner für
                programmierte Aufzeichnungen aus dem Schlafzustand aufgeweckt wird, egal wie der
                Übergang in den Schlafzustand erfolgt ist. Dieses Verhalten kann nicht deaktiviert
                werden. Im ersten Szenario arbeitet der VCR.NET Recording Service wie folgt: für
                jede Aufzeichnung oder Sonderaufgabe wird der Rechner aus dem Schlafzustand aufgeweckt.
                Da dieser Vorgang alleine von System zu System verschieden lange dauert, kann in
                der Konfiguration eine entsprechende Vorlaufzeit programmiert werden. Diese ergibt
                sich aus Erfahrungswerten und ist im Beispiel 30 Sekunden - auf einem anderen Rechner
                reichten 150 Sekunden gerade einmal aus. Diese Zeit hat keinen Einfluss auf den
                Beginn einer Aufzeichnung, sofern sie vernünftig gewählt wurde.
                <br />
                <br />
                Nach Abschluss einer Aufzeichnung oder Sonderaufgabe prüft der VCR.NET Recording
                Service wann die nächste Aktion ansteht. Ist die Wartezeit kürzer als die in der
                Konfiguration eingetragene minimale Pause (im Beispiel 5 Minuten), so wird nicht
                nur auf den Übergang in den Schlafzustand verzichtet sondern zusätzlich sichergestellt,
                dass das Betriebssystem keinen solchen Übergang von sich aus auslöst. Ist die Wartezeit
                allerdings länger, so versetzt der VCR.NET Recording Service den Rechner in den
                Schlafzustand, der in der Konfiguration gewählt wurde.
                <br />
                <br />
                Wird der Rechner aus anderen Gründen in den Schlafzustand versetzt und die nächste
                Aufzeichnung liegt in unmittelbarer Zukunft, so kann es sein, dass VCR.NET den Rechner
                relativ kurzfristig wieder aufweckt. Ist die Option nicht aktiviert, die oben
                im Bild ganz unten zu sehen ist, so versucht der VCR.NET eine minimale Verweildauer
                im Schlafzustand herzustellen (im Beispiel 5 Minuten). Allerdings ist dies nicht in
                jeder Systemkonfiguration möglich und kann im Extremfall dazu führen, dass der Rechner
                gar nicht mehr für eine Folgeaufzeichnung aufgeweckt wird.
                <br />
                <br />
                Verbietet die Konfiguration den Übergang in den Schlafzustand, so werden nach Abschluss
                der Aufzeichnung oder Sonderaufgabe keine weiteren Schritte unternommen. Je nach
                Einstellungen der Energieverwaltung von Windows bleibt der Rechner dann ständig
                an oder geht nach einer bestimmten Wartezeit automatisch in den Schlafzustand. Der
                VCR.NET Recording Service stellt in diesem Szenario lediglich sicher, dass dies
                nicht geschieht, wenn die Zeit im Schlafzustand bis zur nächsten Aufzeichnung oder
                Sonderaufgabe kleiner als die konfigurierte minimale Pause ist.
                <br />
                <br />
                Ein etwas typischeres Szenario ist es wenn der VCR.NET Recording Service auf einem
                Rechner betrieben wird, der auch für andere Aufgaben genutzt wird. Im Normalfall
                ist dann häufig ein Anwender angemeldet und es wäre sehr lästig, wenn der
                Rechner nach einer Aufzeichnung oder einer Sonderaufgabe spontan in den Schlafzustand
                übergeht. Daher ist es dem VCR.NET Recording Service grundsätzlich untersagt, selbstständig
                in den Schlafzustand zu wechseln, so lange noch ein Anwender angemeldet ist.
                <br />
                <br />
                Für einen möglichst einfachen Betrieb in diesem Szenario besitzt
                das <JMSLib.ReactUi.InternalLink view={`${page.route};controlcenter`}>VCR.NET Kontrollzentrum</JMSLib.ReactUi.InternalLink> eine
                besondere Einstellung - es handelt sich bei
                dem Kontrollzentrum um die Anwendung, die normalerweise im so genannten System Tray
                von Windows durch das Kamerasymbol visualisiert wird. Auf Wunsch ist es damit möglich,
                einen Schlafzustand verzögert auslösen zu lassen.
                <ScreenShot description="Kontrollzentrum" name="FAQ/hibernatevcc" />
                Das Kontrollzentrum erkennt wenn der VCR.NET Recording Service eigentlich in den
                Schlafzustand übergehen möchte und führt dann nach der angegebenen Verzögerung genau
                diesen Übergang aus. In der Zwischenzeit kann der Anwender über einen entsprechenden
                Dialog den Vorgang jederzeit abbrechen und normal weiter arbeiten. Ist der Anwender
                gerade nicht aktiv, so greifen zusätzlich die Einstellung der Energieoptionen von
                Windows. Dadurch kann es dann sogar sein, dass der Rechner vor der angegebenen Zeit
                in den Schlafzustand geht, weil Windows keine Aktivitäten des Anwenders mehr wahrnimmt.
                <ScreenShot description="Warten auf den Schlafzustand" name="FAQ/manualsleep" />
                Das VCR.NET Kontrollzentrum hat aber noch
                eine <JMSLib.ReactUi.InternalLink view={`${page.route};controlcenter`}>weitere nützliche Funktion</JMSLib.ReactUi.InternalLink>,
                die nicht deaktiviert werden kann. Aus der Hintergrundfarbe
                des Kamerasymbols kann direkt abgelesen werden, was der VCR.NET Recording Service
                gerade tut. Ist die Farbe <span className="vccBlue">Blau</span>, so wird gerade mindestens eine Aufzeichnung oder
                Sonderaufgabe ausgeführt. Bei <span className="vccYellow">Gelb</span> steht eine Aufzeichnung in naher Zukunft an -
                konkret wie oben konfiguriert näher als die minimale Verweilzeit im Schlafzustand.
                Der Anwender ist angehalten, diese Farbe zu beachten, wenn der Rechner spontan manuell
                in den Schlafzustand versetzt wird.
                <br />
                <br />
                Es ist dem VCR.NET Recording Service nicht möglich, einen solchen manuellen Übergang
                in den Schlafzustand zu verhindern. Tritt ein solcher Fall ein, so werden alle laufenden
                Aufzeichnung und Sonderaufgaben so abgebrochen, als würde der Anwender dies über
                die Web Oberfläche tun - das heißt vor allem, dass die jeweilige Aktion nach einem
                Neustart nicht fortgesetzt wird. Damit der Rechner nicht aufwacht, wird zusätzlich
                die minimale Verweilzeit im Schlafzustand beachtet. Würde eine Aufzeichnung in näherer
                Zukunft anstehen, so wird sich der Start entsprechend verzögern, was bei Aufzeichnungen
                einem Verlust durch zu spätem Starten gleichkommt. Daher die goldene Regel: wird
                der Rechner mit dem VCR.NET Recording Service in den Schlafzustand versetzt während
                das Kontrollzentrum Blau oder Gelb hinterlegt ist, so ist mit einem Verlust an Aufzeichnungsdaten
                zu rechnen.
            </div>;
        }
    }
}
