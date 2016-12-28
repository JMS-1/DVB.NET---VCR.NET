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

                <Ui.Field label="Name:" help="faq;jobsandschedules">
                    <Ui.EditText noui={this.props.noui.name} chars={100} hint="(Optionaler Name der Aufzeichnung)" />
                </Ui.Field>

                <Ui.Field label="Quelle:" help="faq;sourcechooser">
                    <Ui.EditChannel noui={this.props.noui.source} />
                </Ui.Field>

                <Ui.Field label="Besonderheiten:" help="faq;filecontents">
                    <Ui.EditChannelFlags noui={this.props.noui} />
                </Ui.Field>
            </fieldset>;
        }
    }
}
