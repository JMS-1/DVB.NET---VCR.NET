/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    interface IEditChannelStatic {
        noui: App.NoUi.IChannelSelector;
    }

    interface IEditChannelDynamic {
    }

    export class EditChannel extends React.Component<IEditChannelStatic, IEditChannelDynamic> implements App.NoUi.IChannelSelectorSite {
        componentWillMount(): void {
            this.props.noui.setSite(this);
        }

        componentWillUnmount(): void {
            this.props.noui.setSite(undefined);
        }

        render(): JSX.Element {
            return <div className="vcrnet-editchannel">
                <select value={this.props.noui.val()} onChange={this._source}>
                    {this.props.noui.sourceNames.map(s => <option key={`${s.value}`} value={`${s.value}`}>{s.display}</option>)}
                </select>
                <select value={this.props.noui.section()} onChange={this._section}>
                    {this.props.noui.sections.map(s => <option key={s} value={s}>{s}</option>)}
                </select>
                <select value={this.props.noui.type()} onChange={this._type}>
                    {this.props.noui.types.map(t => <option key={t} value={t}>{t}</option>)}
                </select>
                <select value={this.props.noui.encryption()} onChange={this._encryption}>
                    {this.props.noui.encryptions.map(e => <option key={e} value={e}>{e}</option>)}
                </select>
            </div>;
        }

        private _encryption = this.updateEncryption.bind(this);

        private updateEncryption(ev: React.FormEvent): void {
            this.props.noui.encryption((ev.target as HTMLSelectElement).value);
        }

        private _type = this.updateType.bind(this);

        private updateType(ev: React.FormEvent): void {
            this.props.noui.type((ev.target as HTMLSelectElement).value);
        }

        private _section = this.updateSection.bind(this);

        private updateSection(ev: React.FormEvent): void {
            this.props.noui.section((ev.target as HTMLSelectElement).value);
        }

        private _source = this.updateSource.bind(this);

        private updateSource(ev: React.FormEvent): void {
            this.props.noui.val((ev.target as HTMLSelectElement).value);
        }

        refresh(): void {
            this.forceUpdate();
        }
    }
}
