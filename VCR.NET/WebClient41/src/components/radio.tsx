/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IRadioStatic {
        groupName: string;

        isChecked: boolean;

        onClick(): void;
    }

    export class Radio extends React.Component<IRadioStatic, JMSLib.ReactUi.INoDynamicState>{
        render(): JSX.Element {
            return <label className="vcrnet-radio vcrnet-toggleButton" data-vcrnet-checked={this.props.isChecked ? "yes" : null}>
                <input type="RADIO" name={this.props.groupName} defaultChecked={this.props.isChecked} onClick={this.props.onClick} />
                {this.props.children}
            </label>;
        }
    }
}
