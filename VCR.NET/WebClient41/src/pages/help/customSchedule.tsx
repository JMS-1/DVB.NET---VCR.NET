/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class CustomSchedule extends HelpComponent {
        readonly title = "Regeln für die Planung von Aufzeichnungen";

        render(page: App.IPage): JSX.Element {
            return <div>
                Der VCR.NET Recording Service verwendet nach der Installation ein festes Regelwerk zur Planung von
                Aufzeichnungen für den Fall, dass mehrere DVB.NET Geräteprofile verwendet werden. Über die
                Konfiguration<JMSLib.ReactUi.InternalLink view={`${page.application.adminPage.route};rules`} pict="admin" /> kann
                dieses speziellen Bedürfnissen angepasst werden.
                <br />
                <br />
                Für die Aufzeichnungsplanung ermittelt der VCR.NET Recording Service in einer
                ersten Phase Zeitbereiche mit sich überlappenden Aufzeichnungen. Der Zeitraum
                eines solchen Bereiches definiert sich durch den Startzeitpunkt der als erstes
                beginnenden Aufzeichnung und dem Endzeitpunkt der als letztes endenden Aufzeichnung
                darin. Zu jedem Zeitpunkt in einem Bereich ist mindestens eine Aufzeichnung aktiv.
                Alle anderen Aufzeichnungen, die nicht im Bereich erfasst werden, beginnen entweder
                frühestens am Endzeitpunkt oder enden spätestens am Startzeitpunkt.
                <br />
                <br />
                Für jeden Bereich berechnet VCR.NET in der aktuellen Implementierung alle möglichen
                Zuordnungen von Aufzeichnungen zu Geräten - dieses Vorgehen ist bei sehr vielen
                insbesondere gleichartigen Geräten nicht sonderlich clever und wird sich in zukünftigen
                Versionen sicher ändern müssen. Alle diese Zuordnungen werden nach einem
                Regelwerk bewertet und die dadurch definierte beste Zuordnung zur Aufzeichnung verwendet.
                Sind mehrere Zuordnungsvarianten gemäß den Regeln gleichwertig als beste Alternative
                bewertet so wird eine Zuordnung zufällig ausgewählt.
                <br />
                <br />
                Hier in der Konfiguration kann durch eine einfache Abfolge von Regeln die Bewertung
                einer Zuordnung relativ zu einer anderen festgelegt werden. Eine Regel hat dabei immer
                das Format <i>Eigenschaft:Ordnung</i> - Leerzeilen und Zeichen, die auf ein # folgen
                werden gänzlich ignoriert.
                <br />
                <br />
                Das bei der Installation mitgelieferte Regelwerk verwendet folgende Einzelregeln - in
                der hier aufgeführten Reihenfolge:
                <br />
                <br />
                <i><strong>TotalCut:Min</strong></i> Die Eigenschaft <i>TotalCut</i> einer Zuordnung beschreibt die
                gesamte Verspätung von Aufzeichnungen. Wenn etwa zwei überlappende Aufzeichnungen
                auf verschiedenen Quellgruppen (aka <i>Transponder</i>) auf einem Gerät ausgeführt werden,
                so wird die zweite Aufzeichnung verspätet beginnen und somit eventuell die gewünschte
                Aufzeichnung beschnitten. Die Ordnung <i>Min</i> bedeutet hier, dass eine Zuordnung
                umso besser ist, je weniger Verspätung auftritt. Dies ist die erste und damit wichtigste
                Regel im Regelwerk von VCR.NET: soviel der gewünschten Aufzeichnungen ausführen, wie
                nur irgendwie möglich. Diese Regel alleine reicht eigentlich für die meisten Anwendungen
                aus.
                <br />
                <br />
                <i><strong>ResourceCount:Min</strong></i> Mit der Eigenschaft <i>ResourceCount</i> meldet eine
                Zuordnung wie viele Geräte im Zeitbereich eingesetzt werden. VCR.NET sieht eine
                Zuordnung als umso besser an je weniger Geräte verwendet werden. Da es
                sich hier um die zweite Regel im Regelwerk handelt ist sie nur für die Zuordnungen
                relevant, die die geringsten Aufzeichnungsverluste aufweisen.
                <br />
                <br />
                <i><strong>ByPriority:Descending</strong></i> Mit dieser Regel wird die
                Bewertung der Zuordnungen verlassen und auf die Betrachtung einzelner Geräte
                gewechselt. Die Ordnung <i>Descending</i> bedeutet dabei, dass zuerst das DVB.NET
                Gerät mit der höchsten Aufzeichnungspriorität betrachtet wird, dann das mit der
                nächst niedrigeren und so weiter - für die andere Reihenfolge kann die
                Ordnung <i>Ascending</i> verwendet werden. Bei mehreren Geräten gleicher Priorität
                ist die Reihenfolge willkürlich und kann sich mit jedem Start des VCR.NET Dienstes
                ändern.
                <br />
                <br />
                <i><strong>RecordingCount:Max</strong></i> Für ein einzelnes Gerät meldet die
                Eigenschaft <i>RecordingCount</i> die Anzahl der diesem Gerät im Zeitbereich zugeordneten
                Aufzeichnungen. Die Ordnung <i>Max</i> bevorzugt hier Zuordnungen mit mehr Aufzeichnungen
                auf einem Gerät - eine rein willkürliche Entscheidung. In Kombination
                mit <i>ByPriority:Descending</i> bedeutet dies, dass zuerst einmal das Gerät mit
                der höchsten Aufzeichnungspriorität untersucht wird. Eine Zuordnung ist besser als
                eine andere, wenn auf diesem Gerät mehr Aufzeichnungen stattfinden - durch die Position
                im Regelwerk wird dieser Vergleich aber nur vorgenommen, wenn die beiden Zuordnungen
                bereits den minimalen Aufzeichnungsverlust haben und zudem auch eine minimale
                Gerätenutzung garantieren. Ist die Anzahl der Aufzeichnung auf dem höchstpriorisierten
                Gerät identisch, so wird das nächste untersucht und so weiter.
                <br />
                <br />
                <i><strong>ByPriority:End</strong></i> Beendet die Betrachtung einzelner Geräte und wechselt
                zurück in die Bewertung der Zuordnungen an sich. Das nach der Erstinstallation
                verwendete Regelwerk endet an dieser Stelle. Gibt es mehrere gleichwertige Zuordnungen,
                wird nun eine zufällig ausgewählt.<pre>{`
    # Der Verlust an Aufzeichnungszeit muss minimiert werden
    TotalCut:Min
                    
    # Es sollen so wenige Geräte wie möglich verwendet werden
    ResourceCount:Min
                    
    # Die einzelnen Geräte werden mit absteigender Aufzeichnungspriorität untersucht
    ByPriority:Descending
                    
    # Je mehr Aufzeichnungen auf einem Gerät, desto besser
    RecordingCount:Max
                    
    ByPriority:End`}</pre>Die im Folgenden aufgeführten Eigenschaften werden im Standardregelwerk nicht verwendet:
                <br />
                <br />
                <i><strong>SourceCount</strong></i> Ermittelt für ein Gerät wie viele unterschiedliche Quellen
                (aka Sender) im Zeitbereich angesteuert werden. Mit der Ordnung <i>Min</i> würde so eine
                Planung bevorzugt bei der eine Quelle immer vom selben Gerät aufgezeichnet wird. Bei
                vielen gleichzeitigen Aufzeichnungen mit und ohne Überlappung ist diese Bewertung
                allerdings recht unzuverlässig, obwohl sie in Einzelfällen durchaus Sinn macht.
                <br />
                <br />
                <i><strong>ParallelSource</strong></i> ermittelt über einen Planungszeitraum für jede Quelle
                die Gesamtzeit, über die diese auf mehreren Geräten gleichzeitig aufgezeichnet wird. Mit der
                Ordnung <i>Min</i> wird VCR.NET aufgefordert diese Zeit auf ein Minimum zu reduzieren,
                selbst wenn dadurch die Planungspriorität der Geräte überschrieben wird.
                <br />
                <br />
                <i><strong>StartTime:</strong>regelwerk</i> untersucht die relativen Startzeitpunkte von Geräten und
                versucht zu verhindern, dass ein Geräte nach einem anderen aktiviert wird - etwa
                weil bekannt ist, dass bei dem Start der zugehörigen Gerätetreiber auf anderen Geräten
                Störungen ausgelöst werden. Anstelle einer Ordnung wird hier ein Regelwerk aus Einzelregeln
                angegeben, die durch einen senkrechten Strich (|) getrennt sind. Jede Regel beginnt mit dem
                Namen eines Gerätes, darauf folgt das Symbol für <i>kleiner als</i> (&lt;) und dann eine
                ebenfalls durch <i>kleiner als</i> getrennte Liste von Gerätenamen.
                <br />
                <br />
                Ein kleines Beispiel dazu: <i>dev1&lt;dev2&lt;dev3|dev4&lt;dev2</i> bevorzugt Planungen, bei
                denen das Gerät <i>dev1</i> <strong>nicht</strong> nach <i>dev2</i> und <i>dev3</i> gestartet
                wird und <i>dev4</i> <strong>nicht nach</strong> <i>dev2</i>. Der einfachste Fall ist mein
                privates Beispiel mit der <i>Hauppauge Nexus-S</i> unter Windows 7 (32-Bit natürlich) die beim
                Starten Aufzeichnungen anderer Geräte stört: <i>Nexus&lt;*</i> mit der Sonderregel, dass der
                Stern (*) alle (anderen) für VCR.NET freigeschalteten DVB Geräte bezeichnet.
            </div>;
        }
    }
}
