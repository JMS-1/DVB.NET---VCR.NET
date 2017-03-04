/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Anzeige der Details einer Sendung aus der Programmzeitschrift.
    export class GuideEntryInfo extends JMSLib.ReactUi.Component<App.Guide.IGuideInfo> {

        // Oberflächenelemente anlegen.
        render(): JSX.Element {
            return <div className="vcrnet-guideentryinfo">
                <table className="vcrnet-tableIsForm">
                    <tbody>
                        <tr>
                            <td>Name:</td>
                            <td>{this.props.uvm.name}</td>
                        </tr>
                        <tr>
                            <td>Sprache:</td>
                            <td>{this.props.uvm.language}</td>
                        </tr>
                        <tr>
                            <td>Sender:</td>
                            <td>{this.props.uvm.source}</td>
                        </tr>
                        <tr>
                            <td>Beginn:</td>
                            <td>{this.props.uvm.startDisplay}</td>
                        </tr>
                        <tr>
                            <td>Ende:</td>
                            <td>{this.props.uvm.endDisplay}</td>
                        </tr>
                        <tr>
                            <td>Dauer:</td>
                            <td>{this.props.uvm.duration}</td>
                        </tr>
                        <tr>
                            <td>Freigabe:</td>
                            <td>{this.props.uvm.rating}</td>
                        </tr>
                        <tr>
                            <td>Inhalt:</td>
                            <td>{this.props.uvm.content}</td>
                        </tr>
                        <tr>
                            <td>Beschreibung:</td>
                            <td>{this.props.uvm.shortDescription}</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>{this.props.uvm.longDescription}</td>
                        </tr>
                    </tbody>
                </table>
                <div><JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.findSimiliar} /></div>
            </div>;
        }
    }

}
