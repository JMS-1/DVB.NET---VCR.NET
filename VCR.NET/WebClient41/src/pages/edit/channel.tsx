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
                <select value={this.props.uvm.section} onChange={this._section}>
                    {this.props.uvm.sections.map(s => <option key={s} value={s}>{s}</option>)}
                </select>
                <select value={this.props.uvm.type} onChange={this._type} hidden={!this.props.uvm.showFilter}>
                    {this.props.uvm.types.map(t => <option key={t} value={t}>{t}</option>)}
                </select>
                <select value={this.props.uvm.encryption} onChange={this._encryption} hidden={!this.props.uvm.showFilter}>
                    {this.props.uvm.encryptions.map(e => <option key={e} value={e}>{e}</option>)}
                </select>
            </div>;
        }

        // Einschränkung auf die Verschlüsselung ändern.
        private _encryption = this.updateEncryption.bind(this);

        private updateEncryption(ev: React.FormEvent): void {
            this.props.uvm.encryption = (ev.target as HTMLSelectElement).value;
        }

        // Einschränkung auf die Art der Quelle ändern.
        private _type = this.updateType.bind(this);

        private updateType(ev: React.FormEvent): void {
            this.props.uvm.type = (ev.target as HTMLSelectElement).value;
        }

        // Einschränkung auf den Namen der Quelle ändern.
        private _section = this.updateSection.bind(this);

        private updateSection(ev: React.FormEvent): void {
            this.props.uvm.section = (ev.target as HTMLSelectElement).value;
        }

        // Ausgewählte Quelle ändern.
        private _source = this.updateSource.bind(this);

        private updateSource(ev: React.FormEvent): void {
            this.props.uvm.value = (ev.target as HTMLSelectElement).value;
        }
    }
}
