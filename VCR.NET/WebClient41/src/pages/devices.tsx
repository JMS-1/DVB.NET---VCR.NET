/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Devices extends JMSLib.ReactUi.ComponentWithSite<App.IDevicesPage> {
        render(): JSX.Element {
            return <div className="vcrnet-devices">
                <table>
                    <thead>
                        <tr>
                            <td>&nbsp;</td>
                            <td>Beginn</td>
                            <td>Ende</td>
                            <td>Quelle</td>
                            <td>Name</td>
                            <td>Gerät</td>
                            <td>Größe</td>
                        </tr>
                    </thead>
                    <tbody>
                        {this.props.noui.infos.map((i, index) => <Device key={index} page={this.props.noui} noui={i} />)}
                    </tbody>
                </table>
            </div>;
        }
    }

}
