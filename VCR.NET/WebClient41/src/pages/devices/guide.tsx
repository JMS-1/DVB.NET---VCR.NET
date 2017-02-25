/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class DeviceGuide extends JMSLib.ReactUi.ComponentWithSite<App.Devices.IDeviceInfo> {
        render(): JSX.Element {
            return <fieldset className="vcrnet-device-epg">
                {this.props.uvm.guideTime ? <JMSLib.ReactUi.TimeBar uvm={this.props.uvm.guideTime} /> : null}
                {this.props.uvm.guideItem ? <GuideEntryInfo uvm={this.props.uvm.guideItem} /> : null}
            </fieldset>;
        }
    }

}
