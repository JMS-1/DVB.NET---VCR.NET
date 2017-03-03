/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Anzeige der Version (lokal installiert und Online verfügbar) des VCR.NET Recording Service.
    export class VersionCheck extends JMSLib.ReactUi.Component<App.IHomePage> {

        // Oberflächenelemente anzeigen.
        render(): JSX.Element {
            return <li className="vcrnet-home-version">
                <fieldset>
                    Es wird nun im Internet geprüft, ob eine neuere Version des VCR.NET Recording Service angeboten wird.
                    <table>
                        <tbody>
                            <tr>
                                <td>Aktuell installierte Version:</td>
                                <td>{this.props.uvm.currentVersion}</td>
                            </tr>
                            <tr>
                                <td>Aktuell verfügbare Version:</td>
                                <td className={this.props.uvm.versionMismatch ? `vcrnet-warningtext` : undefined}>{this.props.uvm.onlineVersion}</td>
                            </tr>
                        </tbody>
                    </table>
                </fieldset>
            </li>;
        }
    }

}
