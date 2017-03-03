/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Deaktivierung einer einzelnen Ausnahmeregel.
    export class EditException extends JMSLib.ReactUi.Component<App.Edit.IScheduleException> {

        // Erstellt die Oberflächenelemente zur Pflege.
        render(): JSX.Element {
            return <tr className="vcrnet-editexception">
                <td><JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.isActive} /></td>
                <td>{this.props.uvm.dayDisplay}</td>
                <td>{this.props.uvm.startShift} Minute<span>{(this.props.uvm.startShift === 1) ? '' : 'n'}</span></td>
                <td>{this.props.uvm.timeDelta} Minute<span>{(this.props.uvm.timeDelta === 1) ? '' : 'n'}</span></td>
            </tr>;
        }
    }
}