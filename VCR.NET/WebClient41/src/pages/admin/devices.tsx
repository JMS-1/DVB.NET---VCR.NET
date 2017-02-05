/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminDevices extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminDevicesPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-devices">
                <h2>Aktivierung von DVB.NET Geräteprofilen</h2>
                Für den VCR.NET Recording Service kann festgelegt werden, welche der auf dem zugehörigen Rechner
                installierten DVB.NET<HelpLink topic="dvbnet" page={this.props.noui.page} />
                Geräteprofile für Aufzeichnungen verwendet werden dürfen. Eines dieser Geräte
                muss dann als bevorzugtes Gerät festgelegt werden.
                {this.getHelp()}
                <form>
                    <Field page={this.props.noui.page} label={`${this.props.noui.defaultDevice.text}:`}>
                        <JMSLib.ReactUi.EditTextWithList noui={this.props.noui.defaultDevice} />
                    </Field>
                    <table>
                        <thead>
                            <tr>
                                <td>Verwenden</td>
                                <td>Name</td>
                                <td>Priorität</td>
                                <td>Entschlüsselung</td>
                                <td>Quellen</td>
                            </tr>
                        </thead>
                        <tbody>
                            {this.props.noui.devices.map(d => <tr key={d.name}>
                                <td><JMSLib.ReactUi.EditBoolean noui={d.active} /></td>
                                <td>{d.name}</td>
                                <td><JMSLib.ReactUi.EditNumber noui={d.priority} chars={7} /></td>
                                <td><JMSLib.ReactUi.EditNumber noui={d.decryption} chars={7} /></td>
                                <td><JMSLib.ReactUi.EditNumber noui={d.sources} chars={7} /></td>
                            </tr>)}
                        </tbody>
                    </table>
                </form>
                <div>
                    <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.update} />
                </div>
            </div>;
        }

        private getHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Die Tabelle zeigt alle DVB.NET Geräte des Rechners, auf dem der VCR.NET Dienst läuft. Über
                die erste Spalte können beliebig viele davon für Aufzeichnungen durch den VCR.NET Recording
                Service freigeschaltet werden. Eines dieser Geräte muss dann zusätzlich als bevorzugtes
                Gerät festgelegt werden. Dieses bevorzugte Gerät wird zum Beispiel bei neuen Aufträgen
                aber auch beim Aufruf der Programmzeitschrift als Vorauswahl verwendet.
                <br />
                <br />
                Zusätzlich können für jedes Gerät auch einige Geräteparameter festgelegt werden - alternativ
                zur direkten Pflege über
                die <JMSLib.ReactUi.ExternalLink url="http://www.psimarron.net/DVBNET/html/dialogrecording.html">DVB.NET Konfiguration und Administration</JMSLib.ReactUi.ExternalLink>.
                Hier vorgenomme Änderungen werden für alle Geräte übernommen,
                selbst wenn VCR.NET diese nicht verwendet. Grundsätzlich werden Änderungen in der Tabelle erst
                durch eine explizite Bestätigung über die entsprechende Schaltfläche übernommen. Änderungen an
            </InlineHelp>;
        }
    }

}
