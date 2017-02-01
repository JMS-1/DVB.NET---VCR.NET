/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminGuide extends JMSLib.ReactUi.Component<App.IAdminGuidePage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-guide">
                <h1>Einstellungen zum Aufbau der Programmzeitschrift</h1>
            </div>;
        }
    }

}
