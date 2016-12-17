/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IPlanStatic {
        page: App.PlanPage;

        daysToShow: number;
    }

    interface IPlanDynamic {
        jobs?: App.PlanEntry[];
    }

    export class Plan extends React.Component<IPlanStatic, IPlanDynamic> implements App.IPlanSite {
        render(): JSX.Element {
            this.props.page.setSite(this);

            return <div className="vcrnet-plan">
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

        onRefresh(jobs: App.PlanEntry[]): void {
            this.setState({ jobs: jobs });
        }
    }
}
