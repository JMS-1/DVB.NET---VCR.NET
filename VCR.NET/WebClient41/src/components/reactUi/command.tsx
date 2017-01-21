/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    export class Command extends NoUiViewWithSite<App.NoUi.ICommand>  {
        render(): JSX.Element {
            return <button className={`vcrnet-command${this.props.noui.isDangerous ? " vcrnet-dangerous" : ""}`} disabled={!this.props.noui.isEnabled} onClick={() => this.props.noui.execute()}>
                {this.props.noui.text}
            </button>;
        }
    }
}
