/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {
    export class JobData extends JMSLib.ReactUi.Component<App.Edit.IJobEditor>{
        render(): JSX.Element {
            return <fieldset className="vcrnet-jobdata">
                <legend>Daten zum Auftrag</legend>

                <Field page={this.props.noui.page} label={`${this.props.noui.device.text}:`}>
                    <JMSLib.ReactUi.EditTextWithList noui={this.props.noui.device} />
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.deviceLock} />
                </Field>

                <Field page={this.props.noui.page} label={`${this.props.noui.name.text}:`} help="jobsandschedules">
                    <JMSLib.ReactUi.EditText noui={this.props.noui.name} chars={100} hint="(Jeder Auftrag muss einen Namen haben)" />
                </Field>

                <Field page={this.props.noui.page} label={`${this.props.noui.folder.text}:`}>
                    <JMSLib.ReactUi.EditTextWithList noui={this.props.noui.folder} />
                </Field>

                <Field page={this.props.noui.page} label={`${this.props.noui.source.text}:`} help="sourcechooser">
                    <EditChannel noui={this.props.noui.source} />
                </Field>

                <Field page={this.props.noui.page} label={`${this.props.noui.sourceFlags.text}:`} help="filecontents">
                    <EditChannelFlags noui={this.props.noui.sourceFlags} />
                </Field>
            </fieldset>;
        }
    }
}
