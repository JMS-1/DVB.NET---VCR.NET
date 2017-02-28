/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class ParallelRecording extends HelpComponent {
        readonly title = "Planung von Aufzeichnungen";

        render(page: App.IPage): JSX.Element {
            return <div>
                Selbstverständlich ist es keine Problem unabhängig Aufzeichnungen auf mehreren DVB
                Karten zu programmieren, wenn diese dem VCR.NET Recording Service zur Nutzung zur
                Verfügung<JMSLib.ReactUi.InternalLink view={`${page.application.adminPage.route};devices`} pict="admin" /> gestellt
                wurden. Hier soll allerdings beschrieben werden, unter welchen Bedingungen es möglich
                ist, mit einer DVB Hardware zum gleichen Zeitpunkt mehrere Quellen aufzuzeichnen.
                <br />
                <br />
                Zuerst einmal ist festzustellen, dass bei dem digitalen Radio- und Fernsehempfang
                die Quellen (Radio- und Fernsehsender) in Gruppen (Transponder, Frequenzen, ...)
                empfangen werden. Zur Aufzeichnung einer Quelle muss das DVB Gerät erst einmal die
                zugehörige Quellgruppe anwählen - pro Tuner in einer Hardware kann zu jedem Zeitpunkt
                immer nur eine Gruppe angesteuert werden, DVB Hardware mit mehreren Tunern präsentiert
                sich im Allgemeine als mehrere DVB Geräte. Damit werden dann erst einmal automatisch
                alle Quellen dieser Gruppe empfangen. Die Aufzeichnung filtert nun eine dieser Quellen
                aus und verwirft alle anderen empfangenen Daten. Hier setzt der VCR.NET Recording
                Service bei gleichzeitigen Aufzeichnung an: alle Aufzeichnungen, die Quellen aus
                ein und derselben Quellgruppe verwenden, können parallel aufgezeichnet werden. Hier
                einmal ein Beispiel aus dem täglichen Leben. Auf einer der Quellgruppen <em>RTL
                World</em> werden die Sender RTL, RTL2, VOX, SuperRTL und andere empfangen, analog
                auf einer der <em>ProSiebenSat.1</em> Gruppen ProSieben, Kabel1, SAT.1 und andere.
                <br />
                <br />
                Es sollen nun auf einem DVB Gerät folgende Aufzeichnungen programmiert werden:
                <br />
                <ul>
                    <li>RTL am 20. August 2012 von 14:00 bis 16:00</li>
                    <li>RTL2 am 20. August 2012 von 14:30 bis 15:30</li>
                    <li>VOX am 20. August 2012 von 15:45 bis 17:30</li>
                </ul>
                Obwohl sich diese Aufzeichnungen zeitlich überlappen können alle uneingeschränkt
                und in voller Länge aufgezeichnet werden. Sollte nun eine weitere Aufzeichnung einer
                anderen Quellgruppe hinzugefügt werden, so hat der VCR.NET Recording Service verschiedene
                Strategien, die programmierten Wünsche so gut wie möglich zu
                erfüllen. <JMSLib.ReactUi.InternalLink view={`${page.route};customschedule`}>Die
                genauen Regeln</JMSLib.ReactUi.InternalLink> sind etwas komplexer und berücksichtigen
                neben der programmierten Anfangszeit der Aufzeichnungen unter anderem auch die mögliche
                Anzahl vollständig durchgeführter Aufzeichnungen und den Gesamtverlust verspätet
                beginnender Aufzeichnungen.
                <br />
                <br />
                Zusätzlich zu den drei Aufzeichnungen oben wird nun eine Aufzeichnung auf SAT.1
                am gleichen Tag programmiert, die von 17:00 bis 19:00 durchgeführt werden soll.
                Im Zeitraum von 17:00 bis 17:30 gibt es dabei einen Konflikt, bei dem die spätere
                Aufzeichnung auf SAT.1 den kürzeren zieht: sie wird verspätet um 17:30 beginnen.
                <br />
                <br />
                Etwas anders sieht es aus, wenn die SAT.1 Aufzeichnung um 14:15 beginnen soll -
                die Länge von 2 Stunden wird beibehalten, i.e. die Aufzeichnung endet um 16:15.
                Hier werden erst einmal die Aufzeichnungen auf RTL und RTL2 in voller Länge durchgeführt
                - vor allem aufgrund des frühestens Startzeitpunktes der RTL Programmierung und
                der Tatsache, dass die RTL2 Aufzeichnung verlustfrei nebenher laufen kann. Danach
                folgen die letzten 15 Minuten der SAT.1 Aufzeichnung, die also deutlich verspätet
                beginnen wird. Aufgrund des Startzeitpunktes wandert die VOX Aufnahme ganz ans Ende
                und kann erst nach der SAT.1 Aufzeichnung um 16:15 beginnen - es fehlt also eine
                halbe Stunde.
                <br />
                <br />
                Das Ergebnis einer Planung wird im
                Aufzeichnungsplan<JMSLib.ReactUi.InternalLink view={page.application.planPage.route} pict="plan" /> durch
                entsprechende farbige Symbole dargestellt.
            </div>;
        }
    }
}
