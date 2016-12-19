﻿/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IPlanStatic {
        page: App.PlanPage;
    }

    interface IPlanDynamic {
        jobs?: App.PlanEntry[];

        start?: number;

        detailKey?: string;
    }

    export class Plan extends React.Component<IPlanStatic, IPlanDynamic> implements App.IPlanSite {
        render(): JSX.Element {
            this.props.page.setSite(this);

            return <div className="vcrnet-plan">
                Hier sieht man einen Ausschnitt der geplanten Aufzeichnungen für die nächsten Wochen.<HelpLink page="faq;parallelrecording" />
                <InlineHelp title="Erläuterungen zur Bedienung">
                    <p>
                        Über die Datumsauswahl im linken Bereich kann
                        der zeitliche Anfang des angezeigten Ausschnitts festgelegt werden. Das Ende des Ausschnitts ergibt sich daraus
                        und aus der gewünschten Anzahl von zu berücksichtigenden Tagen.<InternalLink view="settings" pict="settings" text="" />
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
                    </table>
                </InlineHelp>
                {
                    (this.state && this.state.start !== undefined) ?
                        <div className="vcrnet-plan-filter">
                            <RadioGroup>
                                {this.props.page.getStartFilter().map(f =>
                                    <Radio
                                        groupName="filterStart"
                                        isChecked={this.state.start === f.index}
                                        onClick={() => this.props.page.filterOnStart(f.index)}>{(f.index === 0) ? "Jetzt" : DateFormatter.getShortDate(f.date)}</Radio>)}
                            </RadioGroup>
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
                                <PlanRow entry={job} key={job.key} detailToggle={() => this.toggleDetail(job)} />,
                                (job.key === this.state.detailKey) ? <DetailRow prefixColumns={1} dataColumns={5} key={`${job.key}Details`} >
                                    [EPGINFO]
                            </DetailRow> : null
                            ])}
                        </tbody>
                    </table> : null
                }
            </div >;
        }

        private toggleDetail(job: App.PlanEntry): void {
            this.setState({ detailKey: (job.key === this.state.detailKey) ? undefined : job.key });
        }

        onRefresh(jobs: App.PlanEntry[], index: number): void {
            this.setState({ jobs: jobs, start: index });
        }
    }
}
