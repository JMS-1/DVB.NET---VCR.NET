/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Pflege der Daten eines Auftrags.
    export class JobData extends JMSLib.ReactUi.Component<App.Edit.IJobEditor>{

        // Oberflächenelement anlegen.
        render(): JSX.Element {
            return <fieldset className="vcrnet-jobdata">
                <legend>Daten zum Auftrag</legend>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.device.text}:`}>
                    <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.device} />
                    <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.deviceLock} />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.name.text}:`} help="jobsandschedules">
                    <JMSLib.ReactUi.EditText uvm={this.props.uvm.name} chars={100} hint="(Jeder Auftrag muss einen Namen haben)" />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.folder.text}:`}>
                    <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.folder} />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.source.text}:`} help="sourcechooser">
                    <EditChannel uvm={this.props.uvm.source} />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.sourceFlags.text}:`} help="filecontents">
                    <EditChannelFlags uvm={this.props.uvm.sourceFlags} />
                </Field>
            </fieldset>;
        }
    }
}
