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
                <td>{this.props.entry.mode ? <Pictogram name={this.props.entry.mode} type="gif" /> : <span>&nbsp;</span>}</td>
                <td>{this.props.entry.mode ? <a href="javascript:void(0)" onClick={this.props.detailToggle}>{this.props.entry.displayStart}</a> : <span>{this.props.entry.displayStart}</span>}</td>
                <td>{this.props.entry.displayEnd}</td>
                <td>{this.props.entry.station}</td>
                <td className="vcrnet-planrow-name">
                    <div><InternalLink view={`edit;id=${this.props.entry.id}`}>{this.props.entry.fullName}</InternalLink></div>
                    <div>{this.props.entry.exceptionInfo ? <a href="javascript:void(0)" onClick={this.props.editToggle}><Pictogram name="exceptOff" /></a> : null}</div>
                </td>
                <td>{this.props.entry.profile}</td>
            </tr>;
        }
    }
}
