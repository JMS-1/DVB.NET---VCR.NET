/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Deaktivierung einer einzelnen Ausnahmeregel.
    export class EditException extends JMSLib.ReactUi.ComponentWithSite<App.IScheduleException> {
        // Erstellt die Oberflächenelemente zur Pflege.
        render(): JSX.Element {
            return <tr className="vcrnet-editexception">
                <td><JMSLib.ReactUi.EditBoolean noui={this.props.noui.isActive} /></td>
                <td>{this.props.noui.dayDisplay}</td>
                <td>{this.props.noui.startShift} Minute<span>{(this.props.noui.startShift === 1) ? '' : 'n'}</span></td>
                <td>{this.props.noui.timeDelta} Minute<span>{(this.props.noui.timeDelta === 1) ? '' : 'n'}</span></td>
            </tr>;
        }
    }
}