/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    interface IDeviceControl extends JMSLib.ReactUi.IComponent<App.IDeviceController> {
        page: App.IDevicesPage;
    }

    export class DeviceControl extends JMSLib.ReactUi.ComponentEx<App.IDeviceController, IDeviceControl> {
        render(): JSX.Element {
            return <fieldset className="vcrnet-device-control">
                <table>
                    <tbody>
                        <tr>
                            <td>Endzeitpunkt:</td>
                            <td>{this.props.noui.end}</td>
                        </tr>
                        <tr>
                            <td>Verbleibende Dauer:</td>
                            <td>{`${this.props.noui.remaining.value} Minute${(this.props.noui.remaining.value === 1) ? `` : `n`}`}</td>
                        </tr>
                    </tbody>
                </table>
                <JMSLib.ReactUi.EditNumberWithSlider noui={this.props.noui.remaining} />
            </fieldset>;
        }
    }

}
