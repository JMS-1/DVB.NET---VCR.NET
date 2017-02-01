/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminSecurity extends JMSLib.ReactUi.Component<App.IAdminSecurityPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-security">
                <h2>Auswahl der Benutzergruppen</h2>
            </div>;
        }
    }

}
