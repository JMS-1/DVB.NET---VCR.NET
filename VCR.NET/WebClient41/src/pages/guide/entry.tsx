/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Anzeige einer Sendung aus der Programmzeitschrift.
    export class GuideEntry extends JMSLib.ReactUi.Component<App.Guide.IGuideEntry> {

        // Oberflächenelemente anlegen.
        render(): JSX.Element {
            return <tr className="vcrnet-guideentry">
                <td>{this.props.uvm.startDisplay}</td>
                <td>{this.props.uvm.endDisplay}</td>
                <td>{this.props.uvm.source}</td>
                <td><JMSLib.ReactUi.InternalLink view={() => this.props.uvm.toggleDetail()}>{this.props.uvm.name}</JMSLib.ReactUi.InternalLink></td>
            </tr>;
        }
    }

}
