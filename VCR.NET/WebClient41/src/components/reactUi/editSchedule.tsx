/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {
    interface IScheduleDataStatic {
        noui: App.NoUi.IScheduleEditor;
    }

    interface IScheduleDataDynamic {
    }

    export class ScheduleData extends React.Component<IScheduleDataStatic, IScheduleDataDynamic>{
        render(): JSX.Element {
            return <fieldset className="vcrnet-scheduledata">
                <legend>Daten zur Aufzeichnung</legend>

                <Ui.Field label={`${this.props.noui.name.text}:`} help="faq;jobsandschedules">
                    <Ui.EditText noui={this.props.noui.name} chars={100} hint="(Optionaler Name der Aufzeichnung)" />
                </Ui.Field>

                <Ui.Field label={`${this.props.noui.source.text}:`} help="faq;sourcechooser">
                    <Ui.EditChannel noui={this.props.noui.source} />
                </Ui.Field>

                <Ui.Field label={`${this.props.noui.sourceFlags.text}:`} help="faq;filecontents">
                    <Ui.EditChannelFlags noui={this.props.noui.sourceFlags} />
                </Ui.Field>

                <Ui.Field label={`${this.props.noui.firstStart.text}:`}>
                    <Ui.EditDay noui={this.props.noui.firstStart} />
                </Ui.Field>

                <Ui.Field label={`${this.props.noui.duration.text}:`}>
                    <Ui.EditDuration noui={this.props.noui.duration} />
                </Ui.Field>

                <Ui.Field label={`${this.props.noui.repeat.text}:`} help="faq;repeatingschedules">
                    <Ui.EditBoolean noui={this.props.noui.onMonday} />
                    <Ui.EditBoolean noui={this.props.noui.onTuesday} />
                    <Ui.EditBoolean noui={this.props.noui.onWednesday} />
                    <Ui.EditBoolean noui={this.props.noui.onThursday} />
                    <Ui.EditBoolean noui={this.props.noui.onFriday} />
                    <Ui.EditBoolean noui={this.props.noui.onSaturday} />
                    <Ui.EditBoolean noui={this.props.noui.onSunday} />
                    {(this.props.noui.repeat.val() !== 0) ? <div>
                        {this.props.noui.lastDay.text}
                        <Ui.EditDay noui={this.props.noui.lastDay} />
                    </div> : null}
                </Ui.Field>
            </fieldset>;
        }
    }
}
