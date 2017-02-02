/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Plan extends JMSLib.ReactUi.ComponentWithSite<App.IPlanPage> {
        render(): JSX.Element {
            var jobs = this.props.noui.jobs;

            return <div className="vcrnet-plan">
                Hier sieht man einen Ausschnitt der geplanten Aufzeichnungen für die nächsten Wochen.<HelpLink page={this.props.noui} topic="parallelrecording" />
                {this.getHelp()}
                <div className="vcrnet-plan-filter">
                    <JMSLib.ReactUi.EditWithButtonList noui={this.props.noui.startFilter} />
                    <JMSLib.ReactUi.CheckBoxCommand noui={this.props.noui.showTasks} />
                </div>
                {
                    jobs ? <table>
                        <thead>
                            <tr>
                                <td>&nbsp;</td>
                                <td>Beginn</td>
                                <td>Ende</td>
                                <td>Quelle</td>
                                <td>Name</td>
                                <td>Gerät</td>
                            </tr>
                        </thead>
                        <tbody>
                            {jobs.map((job, index) => [
                                <PlanRow noui={job} key={index} />,
                                job.showEpg ?
                                    <JMSLib.ReactUi.DetailRow prefixColumns={1} dataColumns={5} key={`${index}Details`}>
                                        [EPGINFO]
                                    </JMSLib.ReactUi.DetailRow> : null,
                                job.showException ?
                                    <JMSLib.ReactUi.DetailRow prefixColumns={1} dataColumns={5} key={`${index}Exceptions`}>
                                        <PlanException noui={job.exception} page={this.props.noui} />
                                    </JMSLib.ReactUi.DetailRow> : null
                            ])}
                        </tbody>
                    </table> : null
                }
            </div >;
        }

        private getHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Über die Datumsauswahl im linken Bereich kann
                der zeitliche Anfang des angezeigten Ausschnitts festgelegt werden. Das Ende des Ausschnitts ergibt sich daraus
                und aus der gewünschten Anzahl von zu berücksichtigenden Tagen.<JMSLib.ReactUi.InternalLink view="settings" pict="settings" />
                <br />
                <br />
                Die beiden Schaltflächen direkt rechts neben der Datumsauswahl erlauben es zusätzlich zu
                den regulären Aufzeichnungen auch die vorgesehenen Zeiten für die Aktualisierung<HelpLink page={this.props.noui} topic="tasks" />
                der Programmzeitschrift oder der
                Senderliste in der Liste darzustellen.
                <br />
                <br />
                Links vor jeder Aufzeichnung in der Liste befindet sich ein kleines Symbol, dass darüber informiert, ob die
                Aufzeichnung wie gewünscht ausgeführt werden kann oder nicht.
                <br />
                <br />
                <table>
                    <tbody>
                        <tr>
                            <td>
                                <JMSLib.ReactUi.Pictogram name="intime" type="gif" />
                            </td>
                            <td>Die Aufzeichnung wird wie programmiert ausgeführt.</td>
                        </tr>
                        <tr>
                            <td>
                                <JMSLib.ReactUi.Pictogram name="late" type="gif" />
                            </td>
                            <td>Die Aufzeichnung beginnt verspätet, eventuell fehlt der Anfang.</td>
                        </tr>
                        <tr>
                            <td>
                                <JMSLib.ReactUi.Pictogram name="lost" type="gif" />
                            </td>
                            <td>Die Aufzeichnung kann nicht ausgeführt werden.</td>
                        </tr>
                    </tbody>
                </table>
                <br />
                Direkt rechts neben dem Symbol wird der Beginn der
                Aufzeichnung als Verweis angezeigt. Wird dieser Verweis angeklickt, so werden weitere Details zur Aufzeichnung
                sichtbar. Je nach vorhandenen Daten wird auch der zugehörige Eintrag der Programmzeitschrift<HelpLink page={this.props.noui} topic="epg" />
                abgerufen und angezeigt.
                <br />
                <br />
                Durch Anwählen des Verweises auf den Namen der Aufzeichnung kann diese bearbeitet werden.
                <br />
                <br />
                <table>
                    <tbody>
                        <tr>
                            <td>
                                <JMSLib.ReactUi.Pictogram name="exceptOff" />
                            </td>
                            <td>
                                Handelt es sich um eine sich wiederholende Aufzeichnung<HelpLink page={this.props.noui} topic="repeatingschedules" />,
                                so kann auch das Symbol direkt rechts neben dem Namen angeklickt werden,
                                um die Ausnahmeregelung für die jeweilige Aufzeichnung anzuzeigen und zu ändern.
                            </td>
                        </tr>
                    </tbody>
                </table>
            </InlineHelp>;
        }
    }

}
