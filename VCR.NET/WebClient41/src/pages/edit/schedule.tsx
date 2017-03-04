/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Pflege der Daten einer Aufzeichnung.
    export class ScheduleData extends JMSLib.ReactUi.Component<App.Edit.IScheduleEditor>{

        // Oberflächenelement anlegen.
        render(): JSX.Element {
            return <fieldset className="vcrnet-scheduledata">
                <legend>Daten zur Aufzeichnung</legend>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.name.text}:`} help="jobsandschedules">
                    <JMSLib.ReactUi.EditText uvm={this.props.uvm.name} chars={100} hint="(Optionaler Name der Aufzeichnung)" />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.source.text}:`} help="sourcechooser">
                    <EditChannel uvm={this.props.uvm.source} />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.sourceFlags.text}:`} help="filecontents">
                    <EditChannelFlags uvm={this.props.uvm.sourceFlags} />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.firstStart.text}:`}>
                    <JMSLib.ReactUi.EditDay uvm={this.props.uvm.firstStart} />
                    {(this.props.uvm.repeat.value !== 0) && <span>
                        <span>{this.props.uvm.lastDay.text}</span>
                        <JMSLib.ReactUi.EditDay uvm={this.props.uvm.lastDay} />
                    </span>}
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.duration.text}:`}>
                    <EditDuration uvm={this.props.uvm.duration} />
                </Field>

                <Field page={this.props.uvm.page} label={`${this.props.uvm.repeat.text}:`} help="repeatingschedules">
                    <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.onMonday} />
                    <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.onTuesday} />
                    <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.onWednesday} />
                    <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.onThursday} />
                    <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.onFriday} />
                    <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.onSaturday} />
                    <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.onSunday} />
                </Field>
            </fieldset>;
        }
    }
}
