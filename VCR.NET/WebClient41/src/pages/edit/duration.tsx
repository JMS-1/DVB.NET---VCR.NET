/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Auswahl der Dauer eine Aufzeichnung über die EIngabe von Start- und Endzeit.
    export class EditDuration extends JMSLib.ReactUi.Component<App.Edit.IDurationEditor>  {

        // Oberflächenelemente anlegen.
        render(): JSX.Element {
            return <div className="vcrnet-editduration">
                <JMSLib.ReactUi.EditTime uvm={this.props.uvm.startTime} /> bis <JMSLib.ReactUi.EditTime uvm={this.props.uvm.endTime} /> Uhr
            </div>;
        }
    }
}
