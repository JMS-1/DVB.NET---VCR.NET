/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    interface ILogDetails extends JMSLib.ReactUi.IComponent<App.Log.ILogEntry> {
    }

    export class LogDetails extends JMSLib.ReactUi.ComponentEx<App.Log.ILogEntry, ILogDetails> {
        render(): JSX.Element {
            return <form className="vcrnet-logentrydetails">
                <fieldset>
                    <legend>Detailinformationen</legend>
                    <table>
                        <tbody>
                            <tr>
                                <td>Beginn:</td>
                                <td>{this.props.noui.start}</td>
                            </tr>
                            <tr>
                                <td>Ende:</td>
                                <td>{this.props.noui.end}</td>
                            </tr>
                            <tr>
                                <td>Quelle:</td>
                                <td>{this.props.noui.source}</td>
                            </tr>
                            <tr>
                                <td>Größe:</td>
                                <td>{this.props.noui.size}</td>
                            </tr>
                            <tr>
                                <td>Primäre Datei:</td>
                                <td>{this.props.noui.primary}</td>
                            </tr>
                            {this.props.noui.hasFiles ?
                                <tr>
                                    <td>Datei ansehen:</td>
                                    <td>{this.props.noui.files.map((f, index) => <JMSLib.ReactUi.ExternalLink key={index} url={f} sameWindow={true}><JMSLib.ReactUi.Pictogram name="recording" /></JMSLib.ReactUi.ExternalLink>)}</td>
                                </tr> : null}
                        </tbody>
                    </table>
                </fieldset>
            </form>;
        }
    }

}
