/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Schnittstelle zur Anzeige einer Schaltfläche.
    interface IButtonCommand extends IComponent<App.ICommand> {
        // Zusätzliche CSS Klassen.
        className?: string;
    }

    // React.Js Komponente zur Anzeige einer Aktion über eine Schaltfläche.
    export class ButtonCommand extends ComponentExWithSite<App.ICommand, IButtonCommand>  {

        // Oberflächenelemente erzeugen.
        render(): JSX.Element {
            return this.props.uvm.isVisible && <div
                className={`jmslib-command${this.props.className ? ` ${this.props.className}` : ``}${this.props.uvm.isDangerous ? ` jmslib-command-dangerous` : ``}${this.props.uvm.isEnabled ? `` : ` jmslib-command-disabled`}`}
                onClick={ev => this.props.uvm.execute()}
                title={this.props.uvm.message}>
                {this.props.uvm.text}
            </div>;
        }

    }

}
