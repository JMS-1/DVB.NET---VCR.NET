/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {
    // React.Js Komponente zur Pflege eines Wahrheitswertes über eine CHECKBOX.
    export class CheckBoxCommand extends Component<App.IFlag>{
        // Erzeugt die Oberflächenelemente.
        render(): JSX.Element {
            return <div
                title=""
                className="jmslib-checkbox jmslib-command"
                data-jmslib-checked={this.props.noui.value ? "yes" : null}
                onClick={() => this.props.noui.value = !this.props.noui.value}>
                {this.props.noui.text}
            </div>;
        }
    }
}
