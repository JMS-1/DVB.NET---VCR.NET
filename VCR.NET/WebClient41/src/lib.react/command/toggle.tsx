/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // React.Js Komponente zur Pflege eines Wahrheitswertes über eine Schaltfläche.
    export class ToggleCommand extends Component<App.IFlag>{

        // Erzeugt die Oberflächenelemente.
        render(): JSX.Element {
            return <div
                className="jmslib-toggle"
                title={this.props.uvm.message}
                data-jmslib-checked={this.props.uvm.value ? "yes" : "no"}
                onClick={() => this.props.uvm.value = !this.props.uvm.value}>
                {this.props.uvm.text}
            </div>;
        }

    }
}
