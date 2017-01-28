/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {
    export class CheckBoxCommand extends Component<App.IValidatedFlag>{
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
