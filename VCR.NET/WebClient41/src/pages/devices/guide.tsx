/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    interface IDeviceGuide extends JMSLib.ReactUi.IComponent<App.IDeviceInfo> {
        page: App.IDevicesPage;
    }

    export class DeviceGuide extends JMSLib.ReactUi.ComponentEx<App.IDeviceInfo, IDeviceGuide> {
        render(): JSX.Element {
            return <fieldset className="vcrnet-device-epg">
                [EPG]
            </fieldset>;
        }
    }

}
