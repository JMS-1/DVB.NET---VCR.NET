/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient {
    interface IEditTextStatic {
        noui: App.NoUi.IStringEditor;

        chars: number;

        hint?: string;
    }

    interface IEditTextDynamic {
        current: string;
    }

    export class EditText extends React.Component<IEditTextStatic, IEditTextDynamic>  {
        private readonly _onChange = this.onChange.bind(this);

        componentWillMount(): void {
            this.componentWillReceiveProps(this.props, undefined);
        }

        componentWillReceiveProps(nextProps: IEditTextStatic, nextContext: any): void {
            this.setState({ current: nextProps.noui.val() });
        }

        render(): JSX.Element {
            return <input
                type="TEXT"
                className="vcrnet-edittext"
                size={this.props.chars}
                value={this.state.current}
                onChange={this._onChange}
                title={this.props.noui.message}
                placeholder={this.props.hint} />;
        }

        private onChange(ev: React.FormEvent): any {
            this.props.noui.val((ev.target as HTMLInputElement).value);
        }
    }
}
