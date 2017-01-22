/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class EditDuration extends JMSLib.ReactUi.Component<App.IDurationEditor>  {
        render(): JSX.Element {
            return <div className="vcrnet-editduration">
                <JMSLib.ReactUi.EditTime noui={this.props.noui.startTime} />
                bis
                <JMSLib.ReactUi.EditTime noui={this.props.noui.endTime} />
                Uhr
            </div>;
        }
    }
}
