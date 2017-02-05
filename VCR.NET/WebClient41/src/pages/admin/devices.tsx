/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminDevices extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminDevicesPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-devices">
                <h2>Aktivierung von DVB.NET Geräteprofilen</h2>
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
            </div>;
        }
    }

}
