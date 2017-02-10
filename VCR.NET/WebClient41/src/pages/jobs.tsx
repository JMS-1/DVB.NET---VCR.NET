/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Jobs extends JMSLib.ReactUi.ComponentWithSite<App.IJobPage>{
        render(): JSX.Element {
            return <div className="vcrnet-jobs">
                Diese Ansicht zeigt alle im VCR.NET Recording Service gespeicherten
                Aufträge.<HelpLink page={this.props.noui} topic="archive" /> Je nach
                Auswahl beschränkt sich die Liste entweder auf die noch aktiven Aufträge
                oder bereits ausgeführte Aufträge, die bereits in das Archiv übertragen wurden.
                {this.getHelp()}
                <div>
                    <JMSLib.ReactUi.ButtonFromList noui={this.props.noui.showArchived} />
                </div>
                <fieldset>
                    {this.props.noui.jobs.map((job, index) => <div key={index}>
                        <div>{job.name}</div>
                        {job.schedules.map(schedule => <JMSLib.ReactUi.InternalLink key={schedule.url} view={schedule.url}>{schedule.name}</JMSLib.ReactUi.InternalLink>)}
                    </div>)}
                </fieldset>
            </div>;
        }

        private getHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                <div>
                    Zu jedem Auftrag wir die Liste der zugeordneten Aufzeichnungen
                    angezeigt.<HelpLink page={this.props.noui} topic="jobsandschedules" /> Durch Auswahl einer solchen Aufzeichnung
                    kann unmittelbar zur Pflege der Daten dieser Aufzeichnung gewechselt werden. Der jeweils erste Eintrag unterhalb
                    des Auftrags selbst wird dabei verwendet, um einem bereits existierenden Auftrag eine ganz neue Aufzeichnung
                    hinzu zu fügen.
                </div>
            </InlineHelp>;
        }
    }

}
