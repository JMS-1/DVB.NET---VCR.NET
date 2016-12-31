/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {
    interface IJobDataStatic {
        noui: App.NoUi.IJobEditor;
    }

    interface IJobDataDynamic {
    }

    export class JobData extends React.Component<IJobDataStatic, IJobDataDynamic>{
        render(): JSX.Element {
            return <fieldset className="vcrnet-jobdata">
                <legend>Daten zum Auftrag</legend>

                <Ui.Field label={`${this.props.noui.device.name}:`}>
                    <Ui.EditTextWithList noui={this.props.noui.device} />
                    <Ui.EditBoolean noui={this.props.noui.deviceLock} />
                </Ui.Field>

                <Ui.Field label={`${this.props.noui.name.name}:`} help="faq;jobsandschedules">
                    <Ui.EditText noui={this.props.noui.name} chars={100} hint="(Jeder Auftrag muss einen Namen haben)" />
                </Ui.Field>

                <Ui.Field label={`${this.props.noui.folder.name}:`}>
                    <Ui.EditTextWithList noui={this.props.noui.folder} />
                </Ui.Field>

                <Ui.Field label={`${this.props.noui.source.name}:`} help="faq;sourcechooser">
                    <Ui.EditChannel noui={this.props.noui.source} />
                </Ui.Field>

                <Ui.Field label={`${this.props.noui.sourceFlags.name}:`} help="faq;filecontents">
                    <Ui.EditChannelFlags noui={this.props.noui.sourceFlags} />
                </Ui.Field>
            </fieldset>;
        }
    }
}
