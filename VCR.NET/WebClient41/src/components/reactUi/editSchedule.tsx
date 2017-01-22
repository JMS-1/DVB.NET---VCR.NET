/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {
    export class ScheduleData extends JMSLib.ReactUi.Component<App.IScheduleEditor>{
        render(): JSX.Element {
            return <fieldset className="vcrnet-scheduledata">
                <legend>Daten zur Aufzeichnung</legend>

                <Ui.Field page={this.props.noui.page} label={`${this.props.noui.name.text}:`} help="jobsandschedules">
                    <JMSLib.ReactUi.EditText noui={this.props.noui.name} chars={100} hint="(Optionaler Name der Aufzeichnung)" />
                </Ui.Field>

                <Ui.Field page={this.props.noui.page} label={`${this.props.noui.source.text}:`} help="sourcechooser">
                    <Ui.EditChannel noui={this.props.noui.source} />
                </Ui.Field>

                <Ui.Field page={this.props.noui.page} label={`${this.props.noui.sourceFlags.text}:`} help="filecontents">
                    <Ui.EditChannelFlags noui={this.props.noui.sourceFlags} />
                </Ui.Field>

                <Ui.Field page={this.props.noui.page} label={`${this.props.noui.firstStart.text}:`}>
                    <Ui.EditDay noui={this.props.noui.firstStart} />
                </Ui.Field>

                <Ui.Field page={this.props.noui.page} label={`${this.props.noui.duration.text}:`}>
                    <Ui.EditDuration noui={this.props.noui.duration} />
                </Ui.Field>

                <Ui.Field page={this.props.noui.page} label={`${this.props.noui.repeat.text}:`} help="repeatingschedules">
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.onMonday} />
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.onTuesday} />
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.onWednesday} />
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.onThursday} />
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.onFriday} />
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.onSaturday} />
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.onSunday} />
                    {(this.props.noui.repeat.value !== 0) ? <div>
                        {this.props.noui.lastDay.text}
                        <Ui.EditDay noui={this.props.noui.lastDay} />
                    </div> : null}
                </Ui.Field>
            </fieldset>;
        }
    }
}
