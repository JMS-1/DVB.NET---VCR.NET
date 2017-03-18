/// <reference path="section.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Konfiguration der Planungsregeln.
    export class AdminRules extends AdminSection<App.Admin.IAdminRulesPage>{

        // Das zugehörige Ui View Model.
        static get uvm(): IAdminSectionFactory<App.Admin.IAdminRulesPage> {
            return App.Admin.RulesSection;
        }

        // Die Überschrift für diesen Bereich.
        protected readonly title = `Regeln für die Planung von Aufzeichnungen`;

        // Oberflächenelement anlegen.
        protected renderSection(): JSX.Element {
            return <div className="vcrnet-admin-rules">
                Der VCR.NET Recording Service verwendet nach der Installation ein festes Regelwerk zur Planung von
                Aufzeichnungen für den Fall, dass mehrere DVB.NET Geräteprofile verwendet werden. Hier ist es
                möglich, dieses Regelwerk<HelpLink topic="customschedule" page={this.props.uvm.page} /> anzupassen
                - auf eigene Gefahr, versteht sich.
                <JMSLib.ReactUi.EditTextArea uvm={this.props.uvm.rules} columns={100} rows={25} />
                {this.getHelp()}
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
