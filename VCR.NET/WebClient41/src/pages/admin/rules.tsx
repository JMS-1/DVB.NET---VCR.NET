/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminRules extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminRulesPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-rules">
                <h2>Regeln für die Planung von Aufzeichnungen</h2>
                Der VCR.NET Recording Service verwendet nach der Installation ein festes Regelwerk zur Planung von
                Aufzeichnungen für den Fall, dass mehrere DVB.NET Geräteprofile verwendet werden. Hier ist es
                möglich, dieses Regelwerk<HelpLink topic="customschedule" page={this.props.noui.page} />
                anzupassen - auf eigene Gefahr, versteht sich.
                <JMSLib.ReactUi.EditTextArea noui={this.props.noui.rules} columns={100} rows={25} />
                {this.getHelp()}
                <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.update} />
            </div>;
        }

        private getHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Für eine Aktualisierung des Regelwerks muss die entsprechende Schaltfläche
                explizit betätigt werden. Dabei wird grundsätzlich ein Neustart des VCR.NET Dienstes
                durchgeführt, selbst wenn keine Veränderungen vorgenommen wurden.
                <br />
                <br />
                Um wieder mit dem fest eingebauten Regelwerk wie nach der Erstinstallation
                zu arbeiten muss einfach nur die Eingabe geleert und eine Aktualisierung
                ausgelöst werden.
            </InlineHelp>;
        }
    }

}
