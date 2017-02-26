/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // Die React.Js Anzeige zur Senderauswahl.
    export class EditChannel extends JMSLib.ReactUi.ComponentWithSite<App.IChannelSelector> implements JMSLib.App.IView {
        // Anzeige erstellen.
        render(): JSX.Element {
            return <div className="vcrnet-editchannel">
                <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.sourceName} /> 
                <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.section} /> 
                {this.props.uvm.showFilter ? <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.type} /> : null}
                {this.props.uvm.showFilter ? <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.encryption} /> : null}
            </div>;
        }
    }
}
