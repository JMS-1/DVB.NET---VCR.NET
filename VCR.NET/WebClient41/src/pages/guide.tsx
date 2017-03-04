/// <reference path="../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Anzeige der Programmzeitschrift.
    export class Guide extends JMSLib.ReactUi.ComponentWithSite<App.IGuidePage> {

        // Oberflächenelemente anlegen.
        render(): JSX.Element {
            return <div className="vcrnet-guide">
                Die Programmzeitschrift<HelpLink page={this.props.uvm} topic="epg" /> zeigt pro Gerät alle
                Sendungen der Quellen, für die eine Sammlung der Daten konfiguriert
                wurde.<HelpLink page={this.props.uvm} topic="epgconfig" /> Über diese Ansicht ist es nicht
                nur möglich, sich die Details einzelner Sendungen anzeigen zu lassen. Vielmehr ist es dabei
                auch sofort möglich, eine neue Aufzeichnung anzulegen.
                {this.getHelp()}
                <form>
                    <fieldset>
                        <legend>Einschränkungen festlegen</legend>
                        <div>
                            <Field page={this.props.uvm} label={`${this.props.uvm.profiles.text}:`}>
                                <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.profiles} />
                            </Field>
                            <Field page={this.props.uvm} label={`${this.props.uvm.sources.text}:`}>
                                <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.sources} />
                            </Field>
                            {this.props.uvm.showSourceType && <JMSLib.ReactUi.SingleSelectButton uvm={this.props.uvm.sourceType} merge={true} />}
                            {this.props.uvm.showEncryption && <JMSLib.ReactUi.SingleSelectButton uvm={this.props.uvm.encrpytion} merge={true} />}
                        </div>
                        <div>
                            <Field page={this.props.uvm} label={`${this.props.uvm.queryString.text}:`}>
                                <JMSLib.ReactUi.EditText uvm={this.props.uvm.queryString} chars={30} />
                            </Field>
                            <JMSLib.ReactUi.ToggleCommand uvm={this.props.uvm.withContent} />
                            <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.resetFilter} />
                            <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.addFavorite} />
                        </div>
                        <div><JMSLib.ReactUi.SingleSelectButton uvm={this.props.uvm.days} /></div>
                        <div><JMSLib.ReactUi.SingleSelectButton uvm={this.props.uvm.hours} /></div>
                    </fieldset>
                </form>
                <GuideNavigation uvm={this.props.uvm} />
                <table className="vcrnet-table">
                    <thead>
                        <tr>
                            <td className="vcrnet-column-start">Beginn</td>
                            <td className="vcrnet-column-end">Ende</td>
                            <td>Quelle</td>
                            <td>Name</td>
                        </tr>
                    </thead>
                    <tbody>{this.props.uvm.entries.map((e, index) => [
                        <GuideEntry key={index} uvm={e} />,
                        e.showDetails &&
                        <JMSLib.ReactUi.DetailRow dataColumns={4} key={`${index}Details`}>
                            <GuideDetails uvm={e} page={this.props.uvm} />
                        </JMSLib.ReactUi.DetailRow>
                    ])}</tbody>
                </table>
                <GuideNavigation uvm={this.props.uvm} />
            </div>;
        }

        // Hilfe zur Bedienung erstellen.
        private getHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Sollten mehrere Geräte eingesetzt werden, so kann für jedes Gerät die Programmzeitschrift separat abgefragt werden.
                Abhängig von der konkret eingesetzten DVB Hardware können sich die Verfügbaren Sender durchaus unterscheiden. Nach
                der Auswahl des Gerätes kann direkt daneben direkt eine Vorauswahl der Quelle erfolgen worauf hin dann nur die
                Sendungen dieser Quelle angezeigt werden. Solange keine Quelle explizit ausgewählt wurde kann die Suche
                auf nur Radio- / nur Fernsehsender respektive nur verschlüsselte / nur frei empfangbare Quellen
                beschränkt werden.
                <br />
                <br />
                In der Grundeinstellung wird der darunter angegebene Suchtext verwendet, um die Anzeige auf die Sendungen zu
                beschränken, bei denen der Suchtext entweder im Namen oder in der Beschreibung vorkommt. Die Suche in der
                Beschreibung kann durch den Schalter neben dem Suchtext auch deaktiviert werden, so dass nur noch Sendungen
                berücksichtigt werden, bei denen der Suchtext im Namen vorkommt.
                <br />
                <br />
                Mit der Auswahl von Tag und Uhrzeit wird die Anzeige der Sendungen zusätzlich auf die Sendungen beschränkt,
                die frühestens zum gewählten Zeitpunkt beginnen. In der Voreinstellung werden alle Sendungen berücksichtigt,
                die zum aktuellen Zeitpunkt noch nicht beendet sind - mit den Schaltflächen 'Jetzt' und 'Neue Suche' kann
                jederzeit in diesen Zustand zurück gewechselt werden.
                <br />
                <br />
                Eine Suchbedingung kann nur dann als neuer Favorit angelegt werden, wenn der Suchtext nicht leer ist -
                ansonsten ist die zugehörige Schaltfläche ausgeblendet. Eine eventuell aktive Vorauswahl des
                Startzeitpunktes wird grundsätzlich nicht in den Favoriten übernommen.
                <br />
                <br />
                Da die Anzahl der Sendungen im Allgemeinen größer ist als die konfigurierte Anzahl der in der Liste anzuzeigenden
                Einträgen ist es über weitere Schaltflächen möglich, zur jeweils nächsten oder vorherigen respektive ersten
                Seite zu wechseln.
                <br />
                <br />
                Durch Anwahl des Namens einer Sendung werden dazu Detailinformationen eingeblendet. Ist die Sendung noch
                nicht vorbei, so kann hierüber auch direkt eine zugehörige Aufzeichnung angelegt werden. Dabei werden die
                konfigurierten Vor- und Nachlaufzeiten berücksichtigt. In der Voreinstellung wird bei dieser Aktion ein
                neuer Auftrag angelegt, es ist allerdings auch möglich zu einem existierenden Auftrag des gerade gewählten
                Geräteprofils eine weitere Aufzeichnung hinzu zu fügen.
            </InlineHelp>;
        }
    }

}
