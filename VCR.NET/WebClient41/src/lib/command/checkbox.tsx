/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {
    interface ICheckBoxCommand {
        isChecked: boolean;

        onToggle: () => void;
    }

    export class CheckBoxCommand extends React.Component<ICheckBoxCommand, IEmpty>{
        render(): JSX.Element {
            return <label className="vcrnet-checkbox vcrnet-toggleButton" data-vcrnet-checked={this.props.isChecked ? "yes" : null}>
                <input
                    type="CHECKBOX"
                    defaultChecked={this.props.isChecked}
                    onClick={this.props.onToggle} />
                {this.props.children}
            </label>;
        }
    }
}
