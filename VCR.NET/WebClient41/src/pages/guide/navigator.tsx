/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {
    export class GuideNavigation extends JMSLib.ReactUi.Component<App.IGuidePageNavigation>  {
        render(): JSX.Element {
            return <div className="vcrnet-epgnavigation">
                <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.firstPage} />
                <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.prevPage} />
                <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.nextPage} />
            </div>;
        }
    }
}
