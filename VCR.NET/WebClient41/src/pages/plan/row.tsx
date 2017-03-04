/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Anzeige einer Aufzeichnung im Aufzeichnungsplan.
    export class PlanRow extends JMSLib.ReactUi.Component<App.Plan.IPlanEntry>  {

        // Oberflächenelemente erstellen.
        render(): JSX.Element {
            return <tr className="vcrnet-planrow">
                <td>{this.props.uvm.mode ? <JMSLib.ReactUi.Pictogram name={this.props.uvm.mode} /> : <span>&nbsp;</span>}</td>
                <td>{this.props.uvm.mode ? <JMSLib.ReactUi.InternalLink view={() => this.props.uvm.toggleDetail(true)}>{this.props.uvm.displayStart}</JMSLib.ReactUi.InternalLink> : <span>{this.props.uvm.displayStart}</span>}</td>
                <td className={this.props.uvm.suspectTime ? `vcrnet-planrow-suspect` : undefined}>{this.props.uvm.displayEnd}</td>
                <td>{this.props.uvm.station}</td>
                <td>{this.props.uvm.editLink ? <JMSLib.ReactUi.InternalLink view={this.props.uvm.editLink}>{this.props.uvm.name}</JMSLib.ReactUi.InternalLink> : <span>{this.props.uvm.name}</span>}</td>
                <td>{this.props.uvm.exception ? <JMSLib.ReactUi.InternalLink view={() => this.props.uvm.toggleDetail(false)} pict={this.props.uvm.exception.exceptionMode} /> : null}</td>
                <td>{this.props.uvm.device}</td>
            </tr>;
        }
    }
}
