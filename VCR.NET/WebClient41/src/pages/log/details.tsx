/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Anzeige der Details eines Protokolleintrags.
    export class LogDetails extends JMSLib.ReactUi.Component<App.Log.ILogEntry> {

        // Oberflächenelemente erstellen.
        render(): JSX.Element {
            return <form className="vcrnet-logentrydetails">
                <fieldset>
                    <legend>Detailinformationen</legend>
                    <table className="vcrnet-tableIsForm">
                        <tbody>
                            <tr>
                                <td>Beginn:</td>
                                <td>{this.props.uvm.start}</td>
                            </tr>
                            <tr>
                                <td>Ende:</td>
                                <td>{this.props.uvm.end}</td>
                            </tr>
                            <tr>
                                <td>Quelle:</td>
                                <td>{this.props.uvm.source}</td>
                            </tr>
                            <tr>
                                <td>Größe:</td>
                                <td>{this.props.uvm.size}</td>
                            </tr>
                            <tr>
                                <td>Primäre Datei:</td>
                                <td>{this.props.uvm.primary}</td>
                            </tr>
                            {this.props.uvm.hasFiles && <tr>
                                <td>Datei ansehen:</td>
                                <td>{this.props.uvm.files.map((f, index) => <JMSLib.ReactUi.ExternalLink key={index} url={f} sameWindow={true}><JMSLib.ReactUi.Pictogram name="recording" /></JMSLib.ReactUi.ExternalLink>)}</td>
                            </tr>}
                        </tbody>
                    </table>
                </fieldset>
            </form>;
        }
    }

}
