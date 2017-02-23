/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class GuideEntry extends JMSLib.ReactUi.Component<App.Guide.IGuideEntry> {
        render(): JSX.Element {
            return <tr className="vcrnet-guideentry">
                <td>{this.props.uvm.startDisplay}</td>
                <td>{this.props.uvm.endDisplay}</td>
                <td>{this.props.uvm.source}</td>
                <td><a href="javascript:void(0)" onClick={() => this.props.uvm.toggleDetail()}>{this.props.uvm.name}</a></td>
            </tr>;
        }
    }

}
