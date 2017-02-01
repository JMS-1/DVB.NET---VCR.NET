/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminSources extends JMSLib.ReactUi.Component<App.IAdminSourcesPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-sources">
                <h1>Aktualisierung der Quellen konfigurieren</h1>
            </div>;
        }
    }

}
