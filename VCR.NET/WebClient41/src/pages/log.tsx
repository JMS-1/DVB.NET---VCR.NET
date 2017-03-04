/// <reference path="../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Anzeige des Protokolls.
    export class Log extends JMSLib.ReactUi.ComponentWithSite<App.ILogPage>{

        // Oberflächenelemente anlegen.
        render(): JSX.Element {
            return <div className="vcrnet-log">
                Für jede Nutzung eines Gerätes erstellt der VCR.NET Recording Service einen
                Protokolleintrag<HelpLink topic="log" page={this.props.uvm} />, der hier eingesehen werden kann.
                Bei überlappenden Aufzeichnung wird ein einziger Eintrag erstellt, der den gesamten Nutzungszeitraum beschreibt.
                {this.getHelp()}
                <form className="vcrnet-bar">
                    <Field page={this.props.uvm} label={`${this.props.uvm.profiles.text}:`}>
                        <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.profiles} />
                        <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.startDay} />
                        <JMSLib.ReactUi.ToggleCommand uvm={this.props.uvm.showGuide} />
                        <JMSLib.ReactUi.ToggleCommand uvm={this.props.uvm.showScan} />
                        <JMSLib.ReactUi.ToggleCommand uvm={this.props.uvm.showLive} />
                    </Field>
                </form>
                <table className="vcrnet-table">
                    <thead>
                        <tr>
                            <td className="vcrnet-column-start">Beginn</td>
                            <td className="vcrnet-column-end">Ende</td>
                            <td>Quelle</td>
                        </tr>
                    </thead>
                    <tbody>
                        {this.props.uvm.entries.map((e, index) => [
                            <tr key={index}>
                                <td><JMSLib.ReactUi.InternalLink view={() => e.showDetail.value = !e.showDetail.value}>{e.start}</JMSLib.ReactUi.InternalLink></td>
                                <td>{e.endTime}</td>
                                <td>{e.source}</td>
                            </tr>,
                            e.showDetail.value && <JMSLib.ReactUi.DetailRow key={`${index}Detail`} dataColumns={3}>
                                <LogDetails uvm={e} />
                            </JMSLib.ReactUi.DetailRow>
                        ])}
                    </tbody>
                </table>
            </div>;
        }

        // Hilfe erstellen.
        private getHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Die Anzeige auf dieser Seite erfolgt immer pro Gerät und Woche, wobei sowohl das Gerät als auch die gewünschte
                Woche über die Auswahllisten vorgegeben werden können. Man beachte dabei, dass gemäß der Konfiguration Protokolle
                nur für einen bestimmten Zeitraum vorgehalten werden, so dass bei der Anzeige vergangener Wochen die Liste
                evntuell leer bleibt.
                <br />
                <br />
                Nach dem Aufruf der Seite werden erst einmal nur die regulären Aufzeichnungen angezeigt. Die Nutzung des
                jeweiligen Gerätes durch Aktualisierungen<HelpLink topic="tasks" page={this.props.uvm} /> und
                den LIVE Zugriff kann durch die entsprechenden Schaltflächen
                neben der Auswahl der Woche eingeblendet werden.
                <br />
                <br />
                Durch Anwahl des jeweiligen Startzeitpunkts eines Protokolleintrags wird die Detailanzeige geöffnet. Handelt
                es sich bei der Nutzung des Geräte um eine oder mehrere reguläre Aufzeichnungen, so sind mit dieser eventuell
                noch nicht gelöschte Aufzeichnungsdateien
                verbunden.<HelpLink topic="filecontents" page={this.props.uvm} /> Durch Anwahl des jeweiligen Symbols können
                diese zur Anzeige durch
                den <JMSLib.ReactUi.ExternalLink url="http://www.psimarron.net/DVBNETViewer/html/vcrfile.html">DVB.NET
                / VCR.NET Viewer</JMSLib.ReactUi.ExternalLink> abgerufen werden.
            </InlineHelp>;
        }
    }

}
