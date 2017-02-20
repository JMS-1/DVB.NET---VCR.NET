/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    interface IDeviceControl extends JMSLib.ReactUi.IComponent<App.IDeviceController> {
        page: App.IDevicesPage;
    }

    export class DeviceControl extends JMSLib.ReactUi.ComponentExWithSite<App.IDeviceController, IDeviceControl> {
        render(): JSX.Element {
            return <fieldset className="vcrnet-device-control">
                {this.props.noui.live ? <div><a href={this.props.noui.live}>Aktuelle Aufzeichnung anschauen</a></div> : null}
                {this.props.noui.timeshift ? <div><a href={this.props.noui.timeshift}>Aufzeichnung zeitversetzt anschauen</a></div> : null}
                {this.props.noui.target ? <div>Aufzeichnung wird aktuell versendet, Empfänger ist {this.props.noui.target}<HelpLink topic="streaming" page={this.props.page} /></div> : null}
                <table>
                    <tbody>
                        <tr>
                            <td>Endzeitpunkt:</td>
                            <td>{this.props.noui.end}</td>
                        </tr>
                        <tr>
                            <td>Verbleibende Dauer:</td>
                            <td>{`${this.props.noui.remaining.value} Minute${(this.props.noui.remaining.value === 1) ? `` : `n`}`}</td>
                        </tr>
                    </tbody>
                </table>
                <JMSLib.ReactUi.EditNumberWithSlider noui={this.props.noui.remaining} />
                <div>
                    <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.stopNow} />
                    <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.update} />
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.noHibernate} />
                </div>
            </fieldset>;
        }
    }

}
