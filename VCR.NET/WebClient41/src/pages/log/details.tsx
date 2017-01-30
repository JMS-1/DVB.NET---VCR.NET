/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    interface ILogDetails extends JMSLib.ReactUi.IComponent<App.ILogEntry> {
    }

    export class LogDetails extends JMSLib.ReactUi.ComponentEx<App.ILogEntry, ILogDetails> {
        render(): JSX.Element {
            return <form className="vcrnet-logentrydetails">
                <fieldset>
                </fieldset>
            </form>;
        }
    }

}
