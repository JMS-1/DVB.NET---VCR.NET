/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // Konfiguration zur Anzeige einer einzelnen Aktivität.
    interface IDevice extends JMSLib.ReactUi.IComponent<App.Devices.IDeviceInfo> {
        // Der zugehörige Navigationsbereich.
        page: App.IDevicesPage;
    }

    // React.Js Komponente zur Anzeige einer Aktivität.
    export class Device extends JMSLib.ReactUi.ComponentEx<App.Devices.IDeviceInfo, IDevice> {

        // Erstellt die Oberflächenelemente.
        render(): JSX.Element {
            var showGuide = this.props.uvm.showGuide;
            var showControl = this.props.uvm.showControl;

            return <tr className="vcrnet-device">
                <td>{this.props.uvm.mode ?
                    (showControl.isReadonly ?
                        <JMSLib.ReactUi.Pictogram name={this.props.uvm.mode} /> :
                        <JMSLib.ReactUi.InternalLink view={() => showControl.value = !showControl.value}><JMSLib.ReactUi.Pictogram name={this.props.uvm.mode} /></JMSLib.ReactUi.InternalLink>) :
                    <span>&nbsp;</span>}
                </td>
                <td>{showGuide.isReadonly ?
                    <span>{this.props.uvm.displayStart}</span> :
                    <JMSLib.ReactUi.InternalLink view={() => showGuide.value = !showGuide.value}>{this.props.uvm.displayStart}</JMSLib.ReactUi.InternalLink>}
                </td>
                <td>{this.props.uvm.displayEnd}</td>
                <td>{this.props.uvm.source}</td>
                <td>{this.props.uvm.id ? <JMSLib.ReactUi.InternalLink view={`${this.props.page.application.editPage.route};id=${this.props.uvm.id}`}>{this.props.uvm.name}</JMSLib.ReactUi.InternalLink> : <span>{this.props.uvm.name}</span>}</td>
                <td>{this.props.uvm.device}</td>
                <td>{this.props.uvm.size}</td>
            </tr>;
        }
    }

}
