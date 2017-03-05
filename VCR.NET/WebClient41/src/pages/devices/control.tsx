/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // Konfiguration der Steuerung einer laufenden Aufzeichnung.
    interface IDeviceControl extends JMSLib.ReactUi.IComponent<App.Devices.IDeviceController> {
        // Der zugehörige Navigationsbereich.
        page: App.IDevicesPage;
    }

    // React.Js Komponente zur Steuerung einer laufenden Aufzeichnung.
    export class DeviceControl extends JMSLib.ReactUi.ComponentExWithSite<App.Devices.IDeviceController, IDeviceControl> {

        // Oberflächenelemente anlegen.
        render(): JSX.Element {
            return <fieldset className="vcrnet-device-control">
                {this.props.uvm.live && <div><a href={this.props.uvm.live}>Aktuelle Aufzeichnung anschauen</a></div>}
                {this.props.uvm.timeshift && <div><a href={this.props.uvm.timeshift}>Aufzeichnung zeitversetzt anschauen</a></div>}
                {this.props.uvm.target && <div className="vcrnet-device-target">Aufzeichnung wird aktuell versendet, Empfänger ist {this.props.uvm.target}<HelpLink topic="streaming" page={this.props.page} /></div>}
                <table className="vcrnet-tableIsForm">
                    <tbody>
                        <tr>
                            <td>Endzeitpunkt:</td>
                            <td>{this.props.uvm.end}</td>
                        </tr>
                        <tr>
                            <td>Verbleibende Dauer:</td>
                            <td>{`${this.props.uvm.remaining.value} Minute${(this.props.uvm.remaining.value === 1) ? `` : `n`}`}</td>
                        </tr>
                    </tbody>
                </table>
                <JMSLib.ReactUi.EditNumberSlider uvm={this.props.uvm.remaining} />
                <div>
                    <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.stopNow} />
                    <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.update} />
                    <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.noHibernate} />
                    <HelpLink topic="hibernation" page={this.props.page} />
                </div>
            </fieldset>;
        }
    }

}
