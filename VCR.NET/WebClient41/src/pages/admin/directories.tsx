/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminDirectories extends JMSLib.ReactUi.Component<App.IAdminDirectoriesPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-directories">
                <h1>Aufzeichnungsverzeichnisse und Dateinamen</h1>
            </div>;
        }
    }

}
