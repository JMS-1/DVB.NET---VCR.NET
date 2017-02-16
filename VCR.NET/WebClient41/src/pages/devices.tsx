/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Devices extends JMSLib.ReactUi.ComponentWithSite<App.IDevicesPage> {
        render(): JSX.Element {
            return <div className="vcrnet-devices">
                <table>
                    <thead>
                        <tr>
                            <td>&nbsp;</td>
                            <td>Beginn</td>
                            <td>Ende</td>
                            <td>Quelle</td>
                            <td>Name</td>
                            <td>Gerät</td>
                            <td>Größe</td>
                        </tr>
                    </thead>
                    <tbody>
                        {this.props.noui.infos.map((i, index) => <tr key={index}>
                            <td>{i.mode ? <JMSLib.ReactUi.Pictogram name={i.mode} type="gif" /> : <span>&nbsp;</span>}</td>
                            <td>{i.start}</td>
                            <td>{i.end}</td>
                            <td>{i.source}</td>
                            <td>{i.id ? <JMSLib.ReactUi.InternalLink view={`${this.props.noui.application.editPage.route};id=${i.id}`}>{i.name}</JMSLib.ReactUi.InternalLink> : <span>{i.name}</span>}</td>
                            <td>{i.device}</td>
                            <td>{i.size}</td>
                        </tr>)}
                    </tbody>
                </table>
            </div>;
        }
    }

}
