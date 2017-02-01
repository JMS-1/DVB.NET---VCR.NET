/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminOther extends JMSLib.ReactUi.Component<App.IAdminOtherPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-other">
                <h1>Sonstige Betriebsparameter</h1>
            </div>;
        }
    }

}
