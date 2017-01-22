/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {
    interface IRadioCommand {
        groupName: string;

        isChecked: boolean;

        onClick(): void;
    }

    export class RadioCommand extends React.Component<IRadioCommand, IEmpty>{
        render(): JSX.Element {
            return <label className="vcrnet-radio vcrnet-toggleButton" data-vcrnet-checked={this.props.isChecked ? "yes" : null}>
                <input
                    type="RADIO"
                    name={this.props.groupName}
                    defaultChecked={this.props.isChecked}
                    onClick={this.props.onClick} />
                {this.props.children}
            </label>;
        }
    }
}
