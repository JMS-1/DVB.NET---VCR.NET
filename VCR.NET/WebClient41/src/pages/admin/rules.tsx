/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminRules extends JMSLib.ReactUi.Component<App.IAdminRulesPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-rules">
                <h2>Regeln für die Planung von Aufzeichnungen</h2>
            </div>;
        }
    }

}
