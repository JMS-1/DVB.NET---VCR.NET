/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class EditDuration extends JMSLib.ReactUi.Component<App.NoUi.IDurationEditor>  {
        render(): JSX.Element {
            return <div className="vcrnet-editduration">
                <EditTime noui={this.props.noui.startTime} />
                bis
                <EditTime noui={this.props.noui.endTime} />
                Uhr
            </div>;
        }
    }
}
