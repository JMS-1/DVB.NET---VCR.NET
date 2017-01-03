/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IPlanStatic {
        page: App.PlanPage;
    }

    interface IPlanDynamic {
        jobs?: App.IPlanEntry[];

        start?: number;

        detailKey?: string;

        showDetails?: boolean;

        showTasks?: boolean;
    }

    export class Plan extends React.Component<IPlanStatic, IPlanDynamic> implements App.IPlanSite {
        componentWillMount(): void {
            this.props.page.setSite(this);
        }

        render(): JSX.Element {
            return <div className="vcrnet-plan">
                Hier sieht man einen Ausschnitt der geplanten Aufzeichnungen für die nächsten Wochen.<HelpLink page="faq;parallelrecording" />
                <InlineHelp title="Erläuterungen zur Bedienung">
                    <p>
                        Über die Datumsauswahl im linken Bereich kann
                        der zeitliche Anfang des angezeigten Ausschnitts festgelegt werden. Das Ende des Ausschnitts ergibt sich daraus
                        und aus der gewünschten Anzahl von zu berücksichtigenden Tagen.<InternalLink view="settings" pict="settings" />
                    </p>
                    <p>
                        Die beiden Schaltflächen direkt rechts neben der Datumsauswahl erlauben es zusätzlich zu
                        den regulären Aufzeichnungen auch die vorgesehenen Zeiten für die Aktualisierung<HelpLink page="faq;tasks" />
                        der Programmzeitschrift oder der
                        Senderliste in der Liste darzustellen.
                    </p>
                    <p>
                        Links vor jeder Aufzeichnung in der Liste befindet sich ein kleines Symbol, dass darüber informiert, ob die
                        Aufzeichnung wie gewünscht ausgeführt werden kann oder nicht.
                    </p>
                    <table>
                        <tbody>
                            <tr>
                                <td>
                                    <Pictogram name="intime" type="gif" />
                                </td>
                                <td>Die Aufzeichnung wird wie programmiert ausgeführt.</td>
                            </tr>
                            <tr>
                                <td>
                                    <Pictogram name="late" type="gif" />
                                </td>
                                <td>Die Aufzeichnung beginnt verspätet, eventuell fehlt der Anfang.</td>
                            </tr>
                            <tr>
                                <td>
                                    <Pictogram name="lost" type="gif" />
                                </td>
                                <td>Die Aufzeichnung kann nicht ausgeführt werden.</td>
                            </tr>
                        </tbody>
                    </table>
                    <p>
                        Direkt rechts neben dem Symbol wird der Beginn der
                        Aufzeichnung als Verweis angezeigt. Wird dieser Verweis angeklickt, so werden weitere Details zur Aufzeichnung
                        sichtbar. Je nach vorhandenen Daten wird auch der zugehörige Eintrag der Programmzeitschrift<HelpLink page="faq;epg" />
                        abgerufen und angezeigt.
                    </p>
                    <p>
                        Durch Anwählen des Verweises auf den Namen der Aufzeichnung kann diese bearbeitet werden.
                    </p>
                    <table>
                        <tbody>
                            <tr>
                                <td>
                                    <Pictogram name="exceptOff" />
                                </td>
                                <td>
                                    Handelt es sich um eine sich wiederholende Aufzeichnung<HelpLink page="faq;repeatingschedules" />,
                                so kann auch das Symbol direkt rechts neben dem Namen angeklickt werden,
                                um die Ausnahmeregelung für die jeweilige Aufzeichnung anzuzeigen und zu ändern.
                            </td>
                            </tr>
                        </tbody>
                    </table>
                </InlineHelp>
                {
                    (this.state && this.state.start !== undefined) ?
                        <div className="vcrnet-plan-filter">
                            <RadioGroup>
                                {this.props.page.getStartFilter().map(f =>
                                    <Radio
                                        key={f.index}
                                        groupName="filterStart"
                                        isChecked={this.state.start === f.index}
                                        onClick={() => this.props.page.filterOnStart(f.index)}>{(f.index === 0) ? "Jetzt" : DateFormatter.getShortDate(f.date)}</Radio>)}
                            </RadioGroup>
                            <CheckBox onToggle={() => this.props.page.toggleTaskFilter()} isChecked={this.state.showTasks}>
                                Aufgaben einblenden
                            </CheckBox>
                        </div> : null
                }
                {
                    (this.state && this.state.jobs) ? <table>
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
                            {this.state.jobs.map(job => [
                                <PlanRow entry={job} key={job.key} detailToggle={() => this.toggleDetail(job, true)} editToggle={() => this.toggleDetail(job, false)} />,
                                ((job.key === this.state.detailKey) && this.state.showDetails) ?
                                    <DetailRow prefixColumns={1} dataColumns={5} key={`${job.key}Details`}>
                                        [EPGINFO]
                                    </DetailRow> : null,
                                ((job.key === this.state.detailKey) && !this.state.showDetails) ?
                                    <DetailRow prefixColumns={1} dataColumns={5} key={`${job.key}Exceptions`}>
                                        [EXTENSIONEDIT]
                                    </DetailRow> : null
                            ])}
                        </tbody>
                    </table> : null
                }
            </div >;
        }

        private toggleDetail(job: App.IPlanEntry, details: boolean): void {
            if ((job.key === this.state.detailKey) && (details === this.state.showDetails))
                this.setState({ detailKey: undefined });
            else
                this.setState({ detailKey: job.key, showDetails: details });
        }

        onRefresh(jobs: App.IPlanEntry[], index: number, showTasks: boolean): void {
            this.setState({ jobs: jobs, start: index, showTasks: showTasks });
        }
    }
}
