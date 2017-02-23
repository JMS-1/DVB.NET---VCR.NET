/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {
    export class GuideNavigation extends JMSLib.ReactUi.Component<App.IGuidePageNavigation>  {
        render(): JSX.Element {
            return <div className="vcrnet-epgnavigation">
                <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.firstPage} />
                <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.prevPage} />
                <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.nextPage} />
            </div>;
        }
    }
}
