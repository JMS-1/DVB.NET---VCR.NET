/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminGuide extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminGuidePage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-guide">
                <h2>Einstellungen zum Aufbau der Programmzeitschrift</h2>
                <form>
                    <Field page={this.props.noui.page} label={`${this.props.noui.hours.text}:`} >
                        <JMSLib.ReactUi.EditMultiButtonList noui={this.props.noui.hours} />
                    </Field>
                </form>
            </div>;
        }
    }

}
