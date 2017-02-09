/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminOther extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminOtherPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-other">
                <h2>Sonstige Betriebsparameter</h2>
                <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.update} />
            </div>;
        }
    }

}
