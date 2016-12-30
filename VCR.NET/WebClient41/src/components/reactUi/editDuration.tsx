/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    interface IEditDurationStatic {
        noui: App.NoUi.IDurationEditor;
    }

    interface IEditDurationDynamic {
    }

    export class EditDuration extends React.Component<IEditDurationStatic, IEditDurationDynamic>  {
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
