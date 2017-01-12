/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    export class EditTime extends NoUiViewWithSite<App.NoUi.ITimeEditor> implements App.NoUi.INoUiSite {
        render(): JSX.Element {
            return <input className="vcrnet-edittime"
                type="TEXT"
                value={this.props.noui.time()}
                title={this.props.noui.error()}
                size={5}
                onChange={this._onChanged} />;
        }

        private readonly _onChanged = this.onChanged.bind(this);

        private onChanged(ev: React.FormEvent): void {
            this.props.noui.time((ev.target as HTMLInputElement).value);
        }
    }
}
