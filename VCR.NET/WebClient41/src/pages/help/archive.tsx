/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class Archive extends HelpComponent {
        readonly title = "Lebenszyklus von Aufträgen";

        render(page: App.IPage): JSX.Element {
            return <div>
                Jede Aufzeichnung hat einen Zeitpunkt, an dem ihre letzte Ausführung abgeschlossen
                ist. Für einzelne Aufzeichnungen ist dies einfach die Summe aus dem programmierten
                Startzeitpunkt und der Aufzeichnungsdauer.
                Für <JMSLib.ReactUi.InternalLink view={`${page.route};repeatingschedules`}>Serienaufzeichnungen</JMSLib.ReactUi.InternalLink> ist
                das etwas komplizierter, da hier auch die Ausnahmeregelungen berücksichtigt werden, 
                das Prinzip ist aber sehr ähnlich.
                <br />
                <br />
                Der VCR.NET Recording Service betrachtet eine Aufzeichnung als abgeschlossen, wenn
                der Zeitpunkt der letzten Ausführung vor der aktuellen Zeit liegt. Dies hat erst
                einmal keine Konsequenzen für die Aufzeichnung. Besitzt allerdings
                ein <JMSLib.ReactUi.InternalLink view={`${page.route};jobsandschedules`}>Auftrag</JMSLib.ReactUi.InternalLink> nur
                abgeschlossene Aufzeichnungen, so wird auch dieser als abgeschlossen betrachtet. 
                Der Auftrag wird dann aus der Liste der
                vorhandenen<JMSLib.ReactUi.InternalLink view={page.application.jobPage.route} pict="jobs" /> Aufträge
                entfernt und in das so genannte
                Archiv<JMSLib.ReactUi.InternalLink view={`${page.application.jobPage.route};archive`} pict="jobs" /> übernommen.
                Die Übertragung ins Archiv kann auch manuell über das Löschen eines Auftrags oder 
                der letzten Aufzeichnung eines Auftrags erfolgen. Die Aufzeichnungen des Auftrags 
                werden dann nicht weiter bei der Planung berücksichtigt.
                <br />
                <br />
                Interessant ist nun, dass ein Auftrag aus dem Archiv jederzeit reaktiviert werden
                kann. Ist er noch nicht abgeschlossen, so reicht das alleinige Öffnen und Speichern
                um die zugehörigen Aufträge wieder in die Planung aufzunehmen. Damit bereits abgeschlossene
                Aufträge wieder berücksichtigt werden können müssen natürlich die Aufzeichnungsdaten
                wie etwa der Startzeitpunkt geeignet verändert werden.
                <br />
                <br />
                Abhängig von der
                Konfiguration<JMSLib.ReactUi.InternalLink view={`${page.application.adminPage.route};other`} pict="admin" /> werden
                die Aufträge im Archiv nach einer gewissen Zeit endgültig gelöscht - dies ist durch 
                manuelles Löschen auch jederzeit vorab möglich. Der VCR.NET Recording Service führt 
                diese Bereinigung allerdings nur durch, wenn der Anwender aktiv die Liste der Aufträge 
                im Archiv
                aufruft<JMSLib.ReactUi.InternalLink view={`${page.application.jobPage.route};archive`} pict="jobs" />.
                Als Kriterium wird der Zeitpunkt verwendet, an dem ein Auftrag in das Archiv übernommen 
                wurde.
                <br />
                <br />
                Unter gewissen Umständen werden Aufträge nicht sofort in das Archiv übertragen,
                obwohl sie faktisch abgeschlossen sind. Das betrifft einfache Aufzeichnungen, die
                während der Ausführung vorzeitig durch den
                Anwender <JMSLib.ReactUi.InternalLink view={`${page.route};editcurrent`}>abgebrochen</JMSLib.ReactUi.InternalLink> wurden.
                Sie wandern erst ins Archiv, wenn die programmierte Endzeit überschritten
                wird - umgekehrt kann die Verlängerung einer laufenden Aufzeichnung auch dazu führen,
                dass ein Auftrag im Archiv erscheint, obwohl eine zugehörige Aufzeichnung noch aktiv
                ist. Bei Serienaufzeichnungen gibt es einen Sicherheitszeitraum von einem Tag, in
                dem eine Aufzeichnung noch im aktiven Bereich verbleibt, obwohl der Tag der letzten
                Ausführung bereits erreicht wurde. In keinem Fall sollten diese Verhaltensweise
                zu Problemen beim Betrieb des VCR.NET Recording Service führen - maximal wird es
                notwendig sein, einen Auftrag an zwei Stellen zu suchen.
            </div>;
        }
    }
}
