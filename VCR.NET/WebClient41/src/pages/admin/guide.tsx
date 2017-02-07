/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminGuide extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminGuidePage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-guide">
                <h2>Einstellungen zum Aufbau der Programmzeitschrift</h2>
                <form>
                    <div>
                        <JMSLib.ReactUi.EditMultiValueList noui={this.props.noui.sources} items={10} />
                        <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.remove} />
                    </div>
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.ukTv} />
                    <Field page={this.props.noui.page} label={`${this.props.noui.device.text}:`}>
                        <JMSLib.ReactUi.EditTextWithList noui={this.props.noui.device} />
                        <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.add} />
                    </Field>
                    <EditChannel noui={this.props.noui.source} />
                    <Field page={this.props.noui.page} label={`${this.props.noui.hours.text}:`} >
                        <JMSLib.ReactUi.EditMultiButtonList noui={this.props.noui.hours} />
                    </Field>
                </form>
            </div>;
        }
    }

}
