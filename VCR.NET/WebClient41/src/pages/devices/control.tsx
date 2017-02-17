/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    interface IDeviceControl extends JMSLib.ReactUi.IComponent<App.IDeviceInfo> {
        page: App.IDevicesPage;
    }

    export class DeviceControl extends JMSLib.ReactUi.ComponentEx<App.IDeviceInfo, IDeviceControl> {
        render(): JSX.Element {
            return <fieldset className="vcrnet-device-control">
                [CONTROL]
            </fieldset>;
        }
    }

}
