/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IJobDataStatic {
        job: App.JobData;
    }

    interface IJobDataDynamic {
    }

    export class JobData extends React.Component<IJobDataStatic, IJobDataDynamic>{
        render(): JSX.Element {
            return <fieldset className="vcrnet-jobdata">
                <legend>Daten zum Auftrag</legend>

                <Ui.Field label="DVB.NET Geräteprofil:">
                    <Ui.EditTextWithList noui={this.props.job.deviceEditor} />
                    <Ui.EditBoolean noui={this.props.job.lockedEditor}>(auf diesem Gerät aufzeichnen)</Ui.EditBoolean>
                </Ui.Field>

                <Ui.Field label="Name:" help="faq;jobsandschedules">
                    <Ui.EditText noui={this.props.job.nameEditor} chars={100} hint="(Jeder Auftrag muss einen Namen haben)" />
                </Ui.Field>

                <Ui.Field label="Verzeichnis:">
                    <Ui.EditTextWithList noui={this.props.job.folderEditor} />
                </Ui.Field>

                <Ui.Field label="Quelle:" help="faq;sourcechooser">
                    <Ui.EditChannel noui={this.props.job.channelSelector} />
                </Ui.Field>
            </fieldset>;
        }
    }
}
