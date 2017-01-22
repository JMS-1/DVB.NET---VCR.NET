/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {
    export class PlanRow extends JMSLib.ReactUi.Component<App.NoUi.IPlanEntry>  {
        render(): JSX.Element {
            return <tr className="vcrnet-planrow">
                <td>{this.props.noui.mode ? <Pictogram name={this.props.noui.mode} type="gif" /> : <span>&nbsp;</span>}</td>
                <td>{this.props.noui.mode ? <a href="javascript:void(0)" onClick={() => this.props.noui.toggleDetail(true)}>{this.props.noui.displayStart}</a> : <span>{this.props.noui.displayStart}</span>}</td>
                <td>{this.props.noui.displayEnd}</td>
                <td>{this.props.noui.station}</td>
                <td className="vcrnet-planrow-name">
                    <div>{this.props.noui.editLink ? <InternalLink view={this.props.noui.editLink}>{this.props.noui.name}</InternalLink> : <span>{this.props.noui.name}</span>}</div>
                    <div>{this.props.noui.exception ? <a href="javascript:void(0)" onClick={() => this.props.noui.toggleDetail(false)}><Pictogram name={this.props.noui.exception.exceptionMode} /></a> : null}</div>
                </td>
                <td>{this.props.noui.device}</td>
            </tr>;
        }
    }
}
