/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IPlanStatic {
        page: App.PlanPage;
    }

    interface IPlanDynamic {
        jobs?: App.PlanEntry[];

        start?: number;
    }

    export class Plan extends React.Component<IPlanStatic, IPlanDynamic> implements App.IPlanSite {
        render(): JSX.Element {
            this.props.page.setSite(this);

            return <div className="vcrnet-plan">
                {(this.state && this.state.start !== undefined) ?
                    <div className="vcrnet-plan-filter">
                        <RadioGroup>
                            {this.props.page.getStartFilter().map(f =>
                                <Radio
                                    groupName="filterStart"
                                    isChecked={this.state.start === f.index}
                                    onClick={() => this.props.page.filterOnStart(f.index)}>{(f.index === 0) ? "Jetzt" : DateFormatter.getShortDate(f.date)}</Radio>)}
                        </RadioGroup>
                    </div> : null}
                {(this.state && this.state.jobs) ? <table>
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
                        {this.state.jobs.map(job => <PlanRow entry={job} key={job.key} />)}
                    </tbody>
                </table> : null}
            </div>;
        }

        onRefresh(jobs: App.PlanEntry[], index: number): void {
            this.setState({ jobs: jobs, start: index });
        }
    }
}
