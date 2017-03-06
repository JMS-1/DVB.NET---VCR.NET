/// <reference path="section.tsx" />

namespace VCRNETClient.Ui {

    // React.js Komponente zur Konfiguration der Geräte.
    export class AdminDevices extends AdminSection<App.Admin.IAdminDevicesPage>{

        // Das zugehörige Ui View Model.
        static readonly uvm = App.Admin.DevicesSection;

        // Die Überschrift für diesen Bereich.
        protected readonly title = `Aktivierung von DVB.NET Geräteprofilen`;

        // Oberflächenelemente anlegen.
        protected renderSection(): JSX.Element {
            return <div className="vcrnet-admin-devices">
                Für den VCR.NET Recording Service kann festgelegt werden, welche der auf dem zugehörigen Rechner
                installierten DVB.NET<HelpLink topic="dvbnet" page={this.props.uvm.page} /> Geräteprofile für
                Aufzeichnungen verwendet werden dürfen. Eines dieser Geräte muss dann als bevorzugtes Gerät
                festgelegt werden.
                {this.getHelp()}
                <form>
                    <Field page={this.props.uvm.page} label={`${this.props.uvm.defaultDevice.text}:`}>
                        <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.defaultDevice} />
                    </Field>
                    <table className="vcrnet-table">
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
                            {this.props.uvm.devices.map(d => <tr key={d.name}>
                                <td><JMSLib.ReactUi.EditBoolean uvm={d.active} /></td>
                                <td>{d.name}</td>
                                <td><JMSLib.ReactUi.EditNumber uvm={d.priority} chars={5} /></td>
                                <td><JMSLib.ReactUi.EditNumber uvm={d.decryption} chars={5} /></td>
                                <td><JMSLib.ReactUi.EditNumber uvm={d.sources} chars={5} /></td>
                            </tr>)}
                        </tbody>
                    </table>
                </form>
            </div>;
        }

        // Allgemeine Erläuterungen.
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
                zur direkten Pflege über die <JMSLib.ReactUi.ExternalLink url="http://www.psimarron.net/DVBNET/html/dialogrecording.html">DVB.NET
                Konfiguration und Administration</JMSLib.ReactUi.ExternalLink>. Hier vorgenomme Änderungen
                werden für alle Geräte übernommen, selbst wenn VCR.NET diese nicht verwendet. Grundsätzlich
                werden Änderungen in der Tabelle erst durch eine explizite Bestätigung über die entsprechende
                Schaltfläche übernommen. Änderungen an den Geräten erfordern fast immer einen Neustart des
                VCR.NET Dienstes.
            </InlineHelp>;
        }
    }

}
