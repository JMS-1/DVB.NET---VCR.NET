/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Konfiguration der Planungsregeln.
    export class AdminRules extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminRulesPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-rules">
                <h2>Regeln für die Planung von Aufzeichnungen</h2>
                Der VCR.NET Recording Service verwendet nach der Installation ein festes Regelwerk zur Planung von
                Aufzeichnungen für den Fall, dass mehrere DVB.NET Geräteprofile verwendet werden. Hier ist es
                möglich, dieses Regelwerk<HelpLink topic="customschedule" page={this.props.uvm.page} /> anzupassen
                - auf eigene Gefahr, versteht sich.
                <JMSLib.ReactUi.EditTextArea uvm={this.props.uvm.rules} columns={100} rows={25} />
                {this.getHelp()}
                <div>
                    <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.update} />
                </div>
            </div>;
        }

        // Hilfe zu den Planungsregeln.
        private getHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Für eine Aktualisierung des Regelwerks muss die entsprechende Schaltfläche explizit
                betätigt werden. Dabei wird grundsätzlich ein Neustart des VCR.NET Dienstes durchgeführt,
                selbst wenn keine Veränderungen vorgenommen wurden.
                <br />
                <br />
                Um wieder mit dem fest eingebauten Regelwerk wie nach der Erstinstallation zu arbeiten
                muss einfach nur die Eingabe geleert und eine Aktualisierung ausgelöst werden.
            </InlineHelp>;
        }
    }

}
