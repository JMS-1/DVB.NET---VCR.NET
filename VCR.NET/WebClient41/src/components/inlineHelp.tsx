/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IInlineHelpStatic {
        title: string;
    }

    interface IInlineHelpDynamic {
        open?: boolean;
    }

    export class InlineHelp extends React.Component<IInlineHelpStatic, IInlineHelpDynamic>{
        private _onSwitch: () => void = this.switchOpen.bind(this);

        render(): JSX.Element {
            var isOpen = this.state && this.state.open;

            return <div className="vcrnet-inline-help" data-is-open={isOpen ? "yes" : "no"} onClick={this._onSwitch}>
                <h1>{this.props.title}</h1>
                {isOpen ? <div>{this.props.children}</div> : null}
            </div>;
        }

        private switchOpen(): void {
            this.setState({ open: !this.state || !this.state.open });
        }
    }
}
