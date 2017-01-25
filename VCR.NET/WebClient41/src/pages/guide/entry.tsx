/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class GuideEntry extends JMSLib.ReactUi.Component<App.IGuideEntry> {
        render(): JSX.Element {
            return <tr className="vcrnet-guideentry">
                <td>{this.props.noui.startDisplay}</td>
                <td>{this.props.noui.endDisplay}</td>
                <td>{this.props.noui.source}</td>
                <td><a href="javascript:void(0)" onClick={() => this.props.noui.toggleDetail()}>{this.props.noui.name}</a></td>
            </tr>;
        }
    }

}
