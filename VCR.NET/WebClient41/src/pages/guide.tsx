/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Guide extends JMSLib.ReactUi.ComponentWithSite<App.IGuidePage> {
        render(): JSX.Element {
            return <div className="vcrnet-guide">
                Die Programmzeitschrift<HelpLink page={this.props.noui} topic="epg" /> zeigt pro Gerät alle
                Sendungen der Quellen, für die eine Sammlung der Daten konfiguriert
                wurde.<HelpLink page={this.props.noui} topic="epgconfig" />
                Über diese Ansicht ist es nicht nur möglich, sich die Details einzelner Sendungen anzeigen
                zu lassen. Vielmehr ist es dabei auch sofort möglich, eine neue Aufzeichnung anzulegen.
                {this.getHelp()}
                <fieldset>
                    <legend>Einschränkungen festlegen</legend>
                </fieldset>
                <GuideNavigation noui={this.props.noui} />
                <table>
                    <thead>
                        <tr>
                            <td>Beginn</td>
                            <td>Ende</td>
                            <td>Quelle</td>
                            <td>Name</td>
                        </tr>
                    </thead>
                    <tbody>
                        {this.props.noui.entries.map((e, index) => <tr key={index}>
                            <td>{e.startDisplay}</td>
                            <td>{e.endDisplay}</td>
                            <td>{e.source}</td>
                            <td>{e.name}</td>
                        </tr>)}
                    </tbody>
                </table>
                <GuideNavigation noui={this.props.noui} />
            </div >;
        }

        private getHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                <p>Sollten mehrere Geräte eingesetzt werden, so kann für jedes Gerät die Programmzeitschrift separat abgefragt werden.
                        Abhängig von der konkret eingesetzten DVB Hardware können sich die Verfügbaren Sender durchaus unterscheiden. Nach
                        der Auswahl des Gerätes kann direkt daneben direkt eine Vorauswahl der Quelle erfolgen worauf hin dann nur die
                        Sendungen dieser Quelle angezeigt werden. Solange keine Quelle explizit ausgewählt wurde kann die Suche
                        auf nur Radio- / nur Fernsehsender respektive nur verschlüsselte / nur frei empfangbare Quellen
                        beschränkt werden.</p>
                <p>In der Grundeinstellung wird der darunter angegebene Suchtext verwendet, um die Anzeige auf die Sendungen zu
                        beschränken, bei denen der Suchtext entweder im Namen oder in der Beschreibung vorkommt. Die Suche in der
                        Beschreibung kann durch den Schalter neben dem Suchtext auch deaktiviert werden, so dass nur noch Sendungen
                        berücksichtigt werden, bei denen der Suchtext im Namen vorkommt.</p>
                <p>Mit der Auswahl von Tag und Uhrzeit wird die Anzeige der Sendungen zusätzlich auf die Sendungen beschränkt,
                        die frühestens zum gewählten Zeitpunkt beginnen. In der Voreinstellung werden alle Sendungen berücksichtigt,
                        die zum aktuellen Zeitpunkt noch nicht beendet sind - mit den Schaltflächen 'Jetzt' und 'Neue Suche' kann
                        jederzeit in diesen Zustand zurück gewechselt werden.</p>
                <p>Eine Suchbedingung kann nur dann als neuer Favorit angelegt werden, wenn der Suchtext nicht leer ist -
                        ansonsten ist die zugehörige Schaltfläche ausgeblendet. Eine eventuell aktive Vorauswahl des
                        Startzeitpunktes wird grundsätzlich nicht in den Favoriten übernommen.</p>
                <p>Da die Anzahl der Sendungen im Allgemeinen größer ist als die konfigurierte Anzahl der in der Liste anzuzeigenden
                        Einträgen ist es über weitere Schaltflächen möglich, zur jeweils nächsten oder vorherigen respektive ersten
                        Seite zu wechseln.</p>
                <p>Durch Anwahl des Namens einer Sendung werden dazu Detailinformationen eingeblendet. Ist die Sendung noch
                        nicht vorbei, so kann hierüber auch direkt eine zugehörige Aufzeichnung angelegt werden. Dabei werden die
                        konfigurierten Vor- und Nachlaufzeiten berücksichtigt. In der Voreinstellung wird bei dieser Aktion ein
                        neuer Auftrag angelegt, es ist allerdings auch möglich zu einem existierenden Auftrag des gerade gewählten
                        Geräteprofils eine weitere Aufzeichnung hinzu zu fügen.</p>
            </InlineHelp>;
        }
    }

}
