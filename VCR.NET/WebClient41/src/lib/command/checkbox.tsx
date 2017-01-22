/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {
    interface ICheckBoxCommand {
        isChecked: boolean;

        onToggle: () => void;
    }

    export class CheckBoxCommand extends React.Component<ICheckBoxCommand, IEmpty>{
        render(): JSX.Element {
            return <label className="jmslib-checkbox jmslib-toggleButton" data-jmslib-checked={this.props.isChecked ? "yes" : null}>
                <input
                    type="CHECKBOX"
                    defaultChecked={this.props.isChecked}
                    onClick={this.props.onToggle} />
                {this.props.children}
            </label>;
        }
    }
}
