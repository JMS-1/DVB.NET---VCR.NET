/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class GuideEntry extends JMSLib.ReactUi.Component<App.IGuideEntry> {
        render(): JSX.Element {
            return <tr className="vcrnet-guideentry">
                <td>{this.props.noui.startDisplay}</td>
                <td>{this.props.noui.endDisplay}</td>
                <td>{this.props.noui.source}</td>
                <td>{this.props.noui.name}</td>
            </tr>;
        }
    }

}
