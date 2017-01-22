/// <reference path="../site.tsx" />

namespace JMSLib.ReactUi {

    export class Command extends NoUiViewWithSite<App.ICommand>  {
        render(): JSX.Element {
            return <button className={`vcrnet-command${this.props.noui.isDangerous ? " vcrnet-dangerous" : ""}`} disabled={!this.props.noui.isEnabled} onClick={() => this.props.noui.execute()}>
                {this.props.noui.text}
            </button>;
        }
    }
}
