/// <reference path="section.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Konfiguration sonstiger Einstellungen.
    export class AdminOther extends AdminSection<App.Admin.IAdminOtherPage>{

        // Das zugehörige Ui View Model.
        static get uvm(): IAdminSectionFactory<App.Admin.IAdminOtherPage> {
            return App.Admin.OtherSection;
        }

        // Die Überschrift für diesen Bereich.
        protected readonly title = `Sonstige Betriebsparameter`;

        // Erstellt die Oberflächenelemente.
        protected renderSection(): JSX.Element {
            return <div className="vcrnet-admin-other">
                Hier handelt es sich um grundsätzliche Betriebsparameter des VCR.NET Dienstes. Änderungen hier erfordern
                üblicherweise ein tieferes Verständnis der Arbeitsweise des VCR.NET Recording Service, da Fehleinstellungen
                durchaus dazu führen können, dass der Dienst nicht mehr funktionsfähig ist.
                <form>
                    {this.getWebHelp()}
                    <Field label={`${this.props.uvm.port.text}:`} page={this.props.uvm.page} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.port} chars={8} />
                    </Field>
                    <div>
                        <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.ssl} />
                    </div>
                    <Field label={`${this.props.uvm.securePort.text}:`} page={this.props.uvm.page} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.securePort} chars={8} />
                    </Field>
                    <div>
                        <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.basicAuth} />
                    </div>
                    {this.getSleepHelp()}
                    <Field label={`${this.props.uvm.hibernation.text}:`} page={this.props.uvm.page} >
                        <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.hibernation} />
                    </Field>
                    <Field label={`${this.props.uvm.preSleep.text}:`} page={this.props.uvm.page} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.preSleep} chars={8} />
                    </Field>
                    <Field label={`${this.props.uvm.minSleep.text}:`} page={this.props.uvm.page} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.minSleep} chars={8} />
                    </Field>
                    <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.ignoreMinSleep} />
                    {this.getLogHelp()}
                    <Field label={`${this.props.uvm.logKeep.text}:`} page={this.props.uvm.page} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.logKeep} chars={8} />
                    </Field>
                    <Field label={`${this.props.uvm.jobKeep.text}:`} page={this.props.uvm.page} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.jobKeep} chars={8} />
                    </Field>
                    <div>
                        <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.noH264PCR} />
                    </div>
                    <div>
                        <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.noMPEG2PCR} />
                    </div>
                    <Field label={`${this.props.uvm.logging.text}:`} page={this.props.uvm.page} >
                        <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.logging} />
                    </Field>
                </form>
                {this.getSaveHelp()}
            </div>;
        }

        // Hilfe zur Konfiguration des Web Servers.
        private getWebHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Der VCR.NET Recording Service ist ein Web Server<HelpLink topic="websettings" page={this.props.uvm.page} /> auf
                Basis der Microsoft ASP.NET / .NET Technologie. Als solcher ist es mindestens notwendig,
                einen TCP/IP Port für die Kommunikation mit den Clients und auch dieser Web Anwendung
                festzulegen. Weitere Einstellungen erlauben das Verschlüsseln der Kommunikation mit dem
                Web Server oder alternative Autorisierungsprotokolle.
            </InlineHelp>;
        }

        // Hilfe zum Schlafzustand.
        private getSleepHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Es kann dem VCR.NET Recording Service gestattet werden, den Rechner nach erfolgten Aufzeichnungen
                in den Schlafzustand zu versetzen.<HelpLink topic="hibernation" page={this.props.uvm.page} /> Unabhängig
                davon wird der VCR.NET Dienst versuchen, den Rechner für Aufzeichnungen aus dem Schlafzustand zu
                wecken, falls dieser nicht erst kurz vorher ausgelöst wurde - dieser Grenzwert kann hier eingestellt
                werden. Da nicht jedes System gleich schnell aus dem Schlafzustand in den Betriebszsutand wecheln
                kann, ist es ferner möglich, eine Vorlaufzeit für das Aufwecken festzulegen.
            </InlineHelp>;
        }

        // Hilfe zur Protokollierung.
        private getLogHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Nach erfolgter Aufzeichnung erstellt der VCR.NET Recording Service automatisch einen
                Protokolleintrag mit den Eckdaten der Gerätenutzung.<HelpLink topic="log" page={this.props.uvm.page} /> Diese
                werden allerdings nur eine begrenzte Zeit vorgehalten und dann automatisch endgültig
                gelöscht. Ähnlich verhält es sich mit vollständig abgeschlossenen
                Aufträgen:<HelpLink topic="archive" page={this.props.uvm.page} /> diese werden für
                einen gewissen Zeitraum archiviert, bevor sie endgültig entfernt werden. Während des
                Verbleibs im Archive können sie jederzeit
                abgefrufen<HelpLink topic="archive" page={this.props.uvm.page} /> und erneut verwendet
                werden.
            </InlineHelp>;
        }

        // Hilfe zum Speichern der Konfiguration.
        private getSaveHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Alle Änderungen werden erst durch explizites Betätigen der Schaltfläche übernommen. Einige
                Änderungen wie etwa die Konfiguration des Web Servers erfordern den Neustart des VCR.NET
                Dienstes.
           </InlineHelp>;
        }
    }

}
