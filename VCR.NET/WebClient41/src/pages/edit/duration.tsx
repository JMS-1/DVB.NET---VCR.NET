/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class EditDuration extends JMSLib.ReactUi.Component<App.Edit.IDurationEditor>  {
        render(): JSX.Element {
            return <div className="vcrnet-editduration">
                <JMSLib.ReactUi.EditTime uvm={this.props.uvm.startTime} />
                bis
                <JMSLib.ReactUi.EditTime uvm={this.props.uvm.endTime} />
                Uhr
            </div>;
        }
    }
}
