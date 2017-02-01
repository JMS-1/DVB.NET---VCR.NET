/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminDevices extends JMSLib.ReactUi.Component<App.IAdminDevicesPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-devices">
                <h1>Aktivierung von DVB.NET Geräteprofilen</h1>
            </div>;
        }
    }

}
