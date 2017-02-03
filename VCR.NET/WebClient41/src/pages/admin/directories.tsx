/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminDirectories extends JMSLib.ReactUi.Component<App.Admin.IAdminDirectoriesPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-directories">
                <h2>Aufzeichnungsverzeichnisse und Dateinamen</h2>
            </div>;
        }
    }

}
