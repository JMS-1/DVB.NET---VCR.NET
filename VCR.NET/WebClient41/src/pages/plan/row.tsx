/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {
    export class PlanRow extends JMSLib.ReactUi.Component<App.Plan.IPlanEntry>  {
        render(): JSX.Element {
            return <tr className="vcrnet-planrow">
                <td>{this.props.uvm.mode ? <JMSLib.ReactUi.Pictogram name={this.props.uvm.mode} type="gif" /> : <span>&nbsp;</span>}</td>
                <td>{this.props.uvm.mode ? <a href="javascript:void(0)" onClick={() => this.props.uvm.toggleDetail(true)}>{this.props.uvm.displayStart}</a> : <span>{this.props.uvm.displayStart}</span>}</td>
                <td className={this.props.uvm.suspectTime ? `vcrnet-planrow-suspect` : undefined}>{this.props.uvm.displayEnd}</td>
                <td>{this.props.uvm.station}</td>
                <td className="vcrnet-planrow-name">
                    <div>{this.props.uvm.editLink ? <JMSLib.ReactUi.InternalLink view={this.props.uvm.editLink}>{this.props.uvm.name}</JMSLib.ReactUi.InternalLink> : <span>{this.props.uvm.name}</span>}</div>
                    <div>{this.props.uvm.exception ? <a href="javascript:void(0)" onClick={() => this.props.uvm.toggleDetail(false)}><JMSLib.ReactUi.Pictogram name={this.props.uvm.exception.exceptionMode} /></a> : null}</div>
                </td>
                <td>{this.props.uvm.device}</td>
            </tr>;
        }
    }
}
