/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminOther extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminOtherPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-other">
                <h2>Sonstige Betriebsparameter</h2>
                Hier handelt es sich um grundsätzliche Betriebsparameter des VCR.NET Dienstes. Änderungen hier erfordern
                üblicherweise ein tieferes Verständnis der Arbeitsweise des VCR.NET Recording Service, da Fehleinstellungen
                durchaus dazu führen können, dass der Dienst nicht mehr funktionsfähig ist.
                <form>
                    {this.getWebHelp()}
                    <Field label={`${this.props.noui.port.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.port} chars={8} />
                    </Field>
                    <div>
                        <JMSLib.ReactUi.EditBoolean noui={this.props.noui.ssl} />
                    </div>
                    <Field label={`${this.props.noui.securePort.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.securePort} chars={8} />
                    </Field>
                    <div>
                        <JMSLib.ReactUi.EditBoolean noui={this.props.noui.basicAuth} />
                    </div>
                    {this.getSleepHelp()}
                    <Field label={`${this.props.noui.hibernation.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.SelectSingleFromList noui={this.props.noui.hibernation} />
                    </Field>
                    <Field label={`${this.props.noui.preSleep.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.preSleep} chars={8} />
                    </Field>
                    <Field label={`${this.props.noui.minSleep.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.minSleep} chars={8} />
                    </Field>
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.ignoreMinSleep} />
                    {this.getLogHelp()}
                    <Field label={`${this.props.noui.logKeep.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.logKeep} chars={8} />
                    </Field>
                    <Field label={`${this.props.noui.jobKeep.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.jobKeep} chars={8} />
                    </Field>
                    <div>
                        <JMSLib.ReactUi.EditBoolean noui={this.props.noui.noH264PCR} />
                    </div>
                    <div>
                        <JMSLib.ReactUi.EditBoolean noui={this.props.noui.noMPEG2PCR} />
                    </div>
                    <Field label={`${this.props.noui.logging.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.SelectSingleFromList noui={this.props.noui.logging} />
                    </Field>
                </form>
                {this.getSaveHelp()}
                <div>
                    <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.update} />
                </div>
            </div>;
        }

        private getWebHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Der VCR.NET Recording Service ist ein Web Server<HelpLink topic="websettings" page={this.props.noui.page} />
                auf Basis der Microsoft ASP.NET / .NET
                Technologie. Als solcher ist es mindestens notwendig, einen TCP/IP Port für die Kommunikation
                mit den Clients und auch dieser Web Anwendung festzulegen. Weitere Einstellungen erlauben
                das Verschlüsseln der Kommunikation mit dem Web Server oder alternative Autorisierungsprotokolle.
            </InlineHelp>;
        }

        private getSleepHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Es kann dem VCR.NET Recording Service gestattet werden, den Rechner nach erfolgten Aufzeichnungen
                in den Schlafzustand zu versetzen.<HelpLink topic="hibernation" page={this.props.noui.page} />
                Unabhängig davon wird der VCR.NET Dienst versuchen, den
                Rechner für Aufzeichnungen aus dem Schlafzustand zu wecken, falls dieser nicht erst kurz
                vorher ausgelöst wurde - dieser Grenzwert kann hier eingestellt werden. Da nicht jedes System
                gleich schnell aus dem Schlafzustand in den Betriebszsutand wecheln kann, ist es fernen
                möglich, eine Vorlaufzeit für das Aufwecken festzulegen.
            </InlineHelp>;
        }

        private getLogHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Nach erfolgter Aufzeichnung erstellt der VCR.NET Recording Service automatisch einen Protokolleintrag
                mit den Eckdaten der Gerätenutzung.<HelpLink topic="log" page={this.props.noui.page} />
                Diese werden allerdings nur eine begrenzte Zeit vorgehalten und dann
                automatisch endgültig gelöscht. Ähnlich verhält es sich mit vollständig abgeschlossenen
                Aufträgen:<HelpLink topic="archive" page={this.props.noui.page} />
                diese werden für einen gewissen Zeitraum archiviert, bevor sie endgültig entfernt werden. Während des Verbleibs
                im Archive können sie jederzeit abgefrufen<HelpLink topic="archive" page={this.props.noui.page} /> und erneut verwendet werden.
            </InlineHelp>;
        }

        private getSaveHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Alle Änderungen werden erst durch explizites Betätigen der Schaltfläche übernommen. Einige
                Änderungen wie etwa die Konfiguration des Web Servers erfordern den Neustart des VCR.NET
                Dienstes.
           </InlineHelp>;
        }
    }

}
