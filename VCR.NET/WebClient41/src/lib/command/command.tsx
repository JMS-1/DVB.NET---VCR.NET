/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class ButtonCommand extends ComponentWithSite<App.ICommand>  {
        render(): JSX.Element {
            return this.props.noui.isVisible ? <button className={`jmslib-command${this.props.noui.isDangerous ? " jmslib-dangerous" : ""}`} disabled={!this.props.noui.isEnabled} onClick={ev => this.onClick(ev)}>
                {this.props.noui.text}
            </button> : null;
        }

        private onClick(ev: React.FormEvent): void {
            ev.preventDefault();

            this.props.noui.execute();
        }
    }
}
