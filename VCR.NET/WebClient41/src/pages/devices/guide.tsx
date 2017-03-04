/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Anzeige des Auszugs der Programmzeitschrift für eine Aktivität.
    export class DeviceGuide extends JMSLib.ReactUi.ComponentWithSite<App.Devices.IDeviceInfo> {

        // Oberflächenelemente erstellen.
        render(): JSX.Element {
            return <fieldset className="vcrnet-device-epg">
                {this.props.uvm.guideTime && <JMSLib.ReactUi.TimeBar uvm={this.props.uvm.guideTime} />}
                {this.props.uvm.guideItem && <GuideEntryInfo uvm={this.props.uvm.guideItem} />}
            </fieldset>;
        }
    }

}
