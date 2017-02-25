/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // Die React.Js Anzeige zur Senderauswahl.
    export class EditChannel extends JMSLib.ReactUi.ComponentWithSite<App.IChannelSelector> implements JMSLib.App.ISite {
        // Anzeige erstellen.
        render(): JSX.Element {
            return <div className="vcrnet-editchannel">
                <select value={this.props.uvm.value} onChange={this._source} title={this.props.uvm.message}>
                    {this.props.uvm.sourceNames.map(s => <option key={`${s.value}`} value={`${s.value}`}>{s.display}</option>)}
                </select>
                <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.section} /> 
                {this.props.uvm.showFilter ? <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.type} /> : null}
                {this.props.uvm.showFilter ? <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.encryption} /> : null}
            </div>;
        }

        // Ausgewählte Quelle ändern.
        private _source = this.updateSource.bind(this);

        private updateSource(ev: React.FormEvent): void {
            this.props.uvm.value = (ev.target as HTMLSelectElement).value;
        }
    }
}
