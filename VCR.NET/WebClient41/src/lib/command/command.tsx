/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // React.Js Komponente zur Anzeige einer Aktion über eine Schaltfläche.
    export class ButtonCommand extends ComponentWithSite<App.ICommand>  {
        // Oberflächenelemente erzeugen.
        render(): JSX.Element {
            return this.props.noui.isVisible ? <div
                className={`jmslib-command${this.props.noui.isDangerous ? ` jmslib-dangerous` : ``}`}
                onClick={ev => this.props.noui.execute()}
                disabled={!this.props.noui.isEnabled}
                title={this.props.noui.message}>
                {this.props.noui.text}
            </div> : null;
        }
    }

}
