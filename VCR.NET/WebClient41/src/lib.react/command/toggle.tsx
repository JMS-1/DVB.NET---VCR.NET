/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // React.Js Komponente zur Pflege eines Wahrheitswertes über eine Schaltfläche.
    export class ToggleCommand extends Component<App.IToggableFlag>{

        // Erzeugt die Oberflächenelemente.
        render(): JSX.Element {
            return <ButtonCommand className={`jmslib-toggle${this.props.uvm.value ? ` jmslib-command-checked` : ``}`} uvm={this.props.uvm.toggle} />;
        }

    }
}
