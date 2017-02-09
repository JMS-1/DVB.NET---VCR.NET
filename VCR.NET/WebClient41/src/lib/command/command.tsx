/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // React.Js Komponente zur Anzeige einer Aktion.
    export class ButtonCommand extends ComponentWithSite<App.ICommand>  {
        // Oberflächenelemente erzeugen.
        render(): JSX.Element {
            return this.props.noui.isVisible ? <button
                className={`jmslib-command${this.props.noui.isDangerous ? " jmslib-dangerous" : ""}`}
                disabled={!this.props.noui.isEnabled}
                onClick={ev => this.onClick(ev)}
                title={this.props.noui.message}>
                {this.props.noui.text}
            </button> : null;
        }

        // Aktion ausführen.
        private onClick(ev: React.FormEvent): void {
            // Bei Schaltflächen ist es wichtig, dass die Defaultaktion des Browsers unterbunden wird.
            ev.preventDefault();

            this.props.noui.execute();
        }
    }

}
