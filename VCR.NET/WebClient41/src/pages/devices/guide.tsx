/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class DeviceGuide extends JMSLib.ReactUi.ComponentWithSite<App.IDeviceInfo> {
        render(): JSX.Element {
            return <fieldset className="vcrnet-device-epg">
                {this.props.noui.guideTime ? <JMSLib.ReactUi.TimeBar noui={this.props.noui.guideTime} /> : null}
                {this.props.noui.guideItem ? <GuideEntryInfo noui={this.props.noui.guideItem} /> : null}
            </fieldset>;
        }
    }

}
