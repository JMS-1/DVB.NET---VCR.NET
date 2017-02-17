/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    interface IDevice extends JMSLib.ReactUi.IComponent<App.IDeviceInfo> {
        page: App.IDevicesPage;
    }

    export class Device extends JMSLib.ReactUi.ComponentEx<App.IDeviceInfo, IDevice> {
        render(): JSX.Element {
            return <tr className="vcrnet-device">
                <td>{this.props.noui.mode ? <JMSLib.ReactUi.Pictogram name={this.props.noui.mode} type="gif" /> : <span>&nbsp;</span>}</td>
                <td>{this.props.noui.start}</td>
                <td>{this.props.noui.end}</td>
                <td>{this.props.noui.source}</td>
                <td>{this.props.noui.id ? <JMSLib.ReactUi.InternalLink view={`${this.props.page.application.editPage.route};id=${this.props.noui.id}`}>{this.props.noui.name}</JMSLib.ReactUi.InternalLink> : <span>{this.props.noui.name}</span>}</td>
                <td>{this.props.noui.device}</td>
                <td>{this.props.noui.size}</td>
            </tr>;
        }
    }

}
