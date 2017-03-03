/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class RepeatingSchedules extends HelpComponent {
        readonly title = "Serienaufzeichnungen und Ausnahmeregelungen";

        render(page: App.IPage): JSX.Element {
            return <div>
                Für eine Aufzeichnung kann angegeben werden, ob diese periodisch wiederholt werden
                soll. Der VCR.NET Recording Service bietet hier die Möglichkeit, die Wochentage
                auszuwählen, an denen die Aufzeichnung durchgeführt werden soll - die jeweilige
                Uhrzeit wird dabei beibehalten. Ergänzend kann der Tag angegeben werden, an dem
                die Aufzeichnung letztmalig auszuführen ist - ohne diese Einstellung wird die Aufzeichnung
                unbegrenzt wiederholt. Diese Eingabe ist nur möglich, wenn mindestens ein Wochentag
                zur Wiederholung aktiviert ist.
                <ScreenShot name="faq/repeating" />
                Als Besonderheit ist zu erwähnen, dass nur die Kombination aus dem Zeitpunktder ersten
                Aufzeichnung und den gewählten Wochentagen die Tage der Aufzeichnung festlegt. Würde im
                gezeigten Beispiel als erster Zeitpunkt etwa der 11. Januar 2013 gewählt, so würde die
                erste Aufzeichnung am 17. Januar 2013 erfolgen, da der Freitag kein gültiger Aufzeichnungstag
                ist.
                <br />
                <br />
                Bei sich wiederholenden Ausstrahlungen ist es oft so, dass diese nicht immer
                zum exakt gleichen Zeitpunkt stattfinden. Kleinere Abweichungen lassen sich durch
                vergrößerte Vor- und Nachlaufzeiten kompensieren. Manchmal kann es aber durch Sondersendungen
                vor der gewünschten Ausstrahlung einmalig zu deutlichen Verschiebungen kommen. Der
                VCR.NET Recording Service bietet hier im
                Aufzeichnungsplan<JMSLib.ReactUi.InternalLink view={page.application.planPage.route} pict="plan" /> die
                Möglichkeit, Ausnahmeregelungen für die einzelnen Aufzeichnungstage zu definieren. Dabei
                kann der Startzeitpunkt verschoben oder die Laufzeit und damit das Ende der Aufzeichnung
                pro geplanter Aufzeichnung individuell verändert werden - auf Wunsch kann auch eine Aufzeichnung ganz
                entfallen.
                <ScreenShot name="faq/exceptions" />
                Wurde eine Aufzeichnung auf diesem Weg aus dem Aufzeichnungsplan entfernt, so kann sie nur über die
                direkte Pflege der Aufzeichnung wieder aktiviert werden. Dazu ist die entsprechende Markierung in
                der Liste der aktiven Ausnahmen zu entfernen und dann die Änderung zu bestätigen. Dieses
                Vorgehen kann auch zur Wiederherstellung der ursprünglichen Planung verwendet werden, wenn nur eine
                einfache Ausnahme definiert worden ist - das ginge allerdings auch direkt aus dem Aufzeichnungsplan
                heraus.
                <ScreenShot name="faq/recover" />
            </div>;
        }
    }
}
