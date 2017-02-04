/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminDevices extends JMSLib.ReactUi.ComponentWithSite<App.IAdminDevicesPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-devices">
                <h2>Aktivierung von DVB.NET Geräteprofilen</h2>
            </div>;
        }
    }

}
