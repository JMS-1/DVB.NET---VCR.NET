/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Jobs extends JMSLib.ReactUi.Component<App.IJobPage>{
        render(): JSX.Element {
            return <div className="vcrnet-jobs">
                <div>
                    <JMSLib.ReactUi.EditWithButtonList noui={this.props.noui.showArchived} />
                </div>
            </div>;
        }
    }

}
