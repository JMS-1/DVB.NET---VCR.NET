/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class VersionCheck extends JMSLib.ReactUi.Component<App.IHomePage> {
        render(): JSX.Element {
            return <li className="vcrnet-home-version">
                <fieldset>
                    Es wird nun im Internet geprüft, ob eine neuere Version des VCR.NET Recording Service angeboten wird.
                    <table>
                        <tbody>
                            <tr>
                                <td>Aktuell installierte Version:</td>
                                <td>{this.props.noui.currentVersion}</td>
                            </tr>
                            <tr>
                                <td>Aktuell verfügbare Version:</td>
                                <td>{this.props.noui.onlineVersion}</td>
                            </tr>
                        </tbody>
                    </table>
                </fieldset>
            </li>;
        }
    }

}
