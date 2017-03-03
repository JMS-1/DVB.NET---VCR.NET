/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Ks Komponente zur Navigation durch die Programmzeitschrift.
    export class GuideNavigation extends JMSLib.ReactUi.Component<App.IGuidePageNavigation>  {

        // Oberflächenelemente anlegen.
        render(): JSX.Element {
            return <div className="vcrnet-epgnavigation">
                <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.firstPage} />
                <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.prevPage} />
                <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.nextPage} />
            </div>;
        }
    }
}
