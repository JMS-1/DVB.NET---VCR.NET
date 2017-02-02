/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient.Ui {
    interface IInlineHelpStatic {
        title: string;
    }

    interface IInlineHelpDynamic {
        open?: boolean;
    }

    export class InlineHelp extends React.Component<IInlineHelpStatic, IInlineHelpDynamic>{
        render(): JSX.Element {
            var isOpen = this.state && this.state.open;

            return <div className="vcrnet-inline-help">
                <h1 onClick={ev => this.switchOpen(ev)}>{this.props.title}</h1>
                {isOpen ? <div>{this.props.children}</div> : null}
            </div>;
        }

        private switchOpen(ev: React.MouseEvent): void {
            this.setState({ open: !this.state || !this.state.open });
        }
    }
}
