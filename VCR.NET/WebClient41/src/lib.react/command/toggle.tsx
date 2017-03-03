/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // React.Js Komponente zur Pflege eines Wahrheitswertes über eine Schaltfläche.
    export class ToggleCommand extends Component<App.IFlag>{

        // Erzeugt die Oberflächenelemente.
        render(): JSX.Element {
            return <div
                className={`jmslib-toggle jmslib-command${this.props.uvm.value ? ` jmslib-command-checked` : ``}`}
                title={this.props.uvm.message}
                onClick={() => this.props.uvm.value = !this.props.uvm.value}>
                {this.props.uvm.text}
            </div>;
        }

    }
}
