/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminDirectories extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminDirectoriesPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-directories">
                <h2>Aufzeichnungsverzeichnisse und Dateinamen</h2>
                <JMSLib.ReactUi.EditMultiValueList noui={this.props.noui.directories} />
                <Field page={this.props.noui.page} label={this.props.noui.pattern.text}>
                    <JMSLib.ReactUi.EditText noui={this.props.noui.pattern} chars={40} />
                </Field>
            </div>;
        }
    }

}
