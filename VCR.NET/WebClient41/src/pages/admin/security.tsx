/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminSecurity extends JMSLib.ReactUi.Component<App.IAdminSecurityPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-security">
                <h1>Auswahl der Benutzergruppen</h1>
            </div>;
        }
    }

}
