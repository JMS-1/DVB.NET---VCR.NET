/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    interface ICommandStatic {
        noui: App.NoUi.ICommand;
    }

    export class Command extends React.Component<ICommandStatic, INoDynamicState>  {
        render(): JSX.Element {
            return <button className="vcrnet-command" disabled={!this.props.noui.isEnabled()} onClick={() => this.props.noui.execute()}>
                {this.props.noui.text}
            </button>;
        }
    }
}
