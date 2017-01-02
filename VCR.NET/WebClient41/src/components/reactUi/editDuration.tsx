/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    export class EditDuration extends NoUiView<App.NoUi.IDurationEditor>  {
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
