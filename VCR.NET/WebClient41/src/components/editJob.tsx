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
                <Field label="Name:" help="faq;jobsandschedules">
                    <EditText noui={this.props.job.nameEditor} chars={100} hint="(Jeder Auftrag muss einen Namen haben)" />
                </Field>
            </fieldset>;
        }
    }
}
