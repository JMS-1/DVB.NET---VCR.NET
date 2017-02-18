/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {
    // React.Js Komponente zur Pflege eines Wahrheitswertes über eine CHECKBOX.
    export class CheckBoxCommand extends Component<App.IFlag>{
        // Erzeugt die Oberflächenelemente.
        render(): JSX.Element {
            return <label className="jmslib-checkbox jmslib-toggleButton" data-jmslib-checked={this.props.noui.value ? "yes" : null}>
                <input
                    type="CHECKBOX"
                    checked={this.props.noui.value}
                    onChange={() => this.props.noui.value = !this.props.noui.value} />
                {this.props.noui.text}
            </label>;
        }
    }
}
