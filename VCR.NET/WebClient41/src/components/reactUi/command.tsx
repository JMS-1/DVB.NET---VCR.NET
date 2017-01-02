/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    export class Command extends NoUiView<App.NoUi.ICommand>  {
        render(): JSX.Element {
            return <button className="vcrnet-command" disabled={!this.props.noui.isEnabled()} onClick={() => this.props.noui.execute()}>
                {this.props.noui.text}
            </button>;
        }
    }
}
