/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // React.Js Komponente zur Anzeige einer Aktion über eine Schaltfläche.
    export class ButtonCommand extends ComponentWithSite<App.ICommand>  {

        // Oberflächenelemente erzeugen.
        render(): JSX.Element {
            return this.props.uvm.isVisible ? <div
                className={`jmslib-command${this.props.uvm.isDangerous ? ` jmslib-dangerous` : ``}`}
                data-jmslib-disabled={this.props.uvm.isEnabled ? `no` : `yes`}
                onClick={ev => this.props.uvm.execute()}
                title={this.props.uvm.message}>
                {this.props.uvm.text}
            </div> : null;
        }

    }

}
