/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IPlanRowStatic {
        entry: App.PlanEntry;

        detailToggle: () => void;

        editToggle: () => void;
    }

    interface IPlanRowDynamic {
    }

    export class PlanRow extends React.Component<IPlanRowStatic, IPlanRowDynamic>  {
        render(): JSX.Element {
            return <tr className="vcrnet-planrow">
                <td><Pictogram name={this.props.entry.mode} type="gif" /></td>
                <td><a href="javascript:void(0)" onClick={this.props.detailToggle}>{this.props.entry.displayStart}</a></td>
                <td>{this.props.entry.displayEnd}</td>
                <td>{this.props.entry.station}</td>
                <td className="vcrnet-planrow-name">
                    <div>{this.props.entry.fullName}</div>
                    <div>{this.props.entry.exceptionInfo ? <a href="javascript:void(0)" onClick={this.props.editToggle}><Pictogram name="exceptOff" /></a> : null}</div>
                </td>
                <td>{this.props.entry.profile}</td>
            </tr>;
        }
    }
}
