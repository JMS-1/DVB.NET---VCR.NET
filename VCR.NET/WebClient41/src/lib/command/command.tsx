/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class ButtonCommand extends ComponentWithSite<App.ICommand>  {
        render(): JSX.Element {
            return <button className={`jmslib-command${this.props.noui.isDangerous ? " jmslib-dangerous" : ""}`} disabled={!this.props.noui.isEnabled} onClick={() => this.props.noui.execute()}>
                {this.props.noui.text}
            </button>;
        }
    }
}
