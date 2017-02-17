/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    interface IDevice extends JMSLib.ReactUi.IComponent<App.IDeviceInfo> {
        page: App.IDevicesPage;
    }

    export class Device extends JMSLib.ReactUi.ComponentEx<App.IDeviceInfo, IDevice> {
        render(): JSX.Element {
            var showGuide = this.props.noui.showGuide;
            var showControl = this.props.noui.showControl;

            return <tr className="vcrnet-device">
                <td>{this.props.noui.mode ?
                    (showControl.isReadonly ?
                        <JMSLib.ReactUi.Pictogram name={this.props.noui.mode} type="gif" /> :
                        <JMSLib.ReactUi.InternalLink view={() => showControl.value = !showControl.value}><JMSLib.ReactUi.Pictogram name={this.props.noui.mode} type="gif" /></JMSLib.ReactUi.InternalLink>) :
                    <span>&nbsp;</span>}
                </td>
                <td>{showGuide.isReadonly ?
                    <span>{this.props.noui.displayStart}</span> :
                    <JMSLib.ReactUi.InternalLink view={() => showGuide.value = !showGuide.value}>{this.props.noui.displayStart}</JMSLib.ReactUi.InternalLink>}
                </td>
                <td>{this.props.noui.displayEnd}</td>
                <td>{this.props.noui.source}</td>
                <td>{this.props.noui.id ? <JMSLib.ReactUi.InternalLink view={`${this.props.page.application.editPage.route};id=${this.props.noui.id}`}>{this.props.noui.name}</JMSLib.ReactUi.InternalLink> : <span>{this.props.noui.name}</span>}</td>
                <td>{this.props.noui.device}</td>
                <td>{this.props.noui.size}</td>
            </tr>;
        }
    }

}
