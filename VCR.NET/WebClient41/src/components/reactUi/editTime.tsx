/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    interface IEditTimeStatic {
        noui: App.NoUi.ITimeEditor;
    }

    interface IEditTimeDynamic {
    }

    export class EditTime extends React.Component<IEditTimeStatic, IEditTimeDynamic> implements App.NoUi.ITimeEditorSite {
        componentWillMount(): void {
            this.props.noui.setSite(this);
        }

        componentWillUnmount(): void {
            this.props.noui.setSite(undefined);
        }

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

        refresh(): void {
            this.forceUpdate();
        }
    }
}
