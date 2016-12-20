/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface ICheckBoxStatic {
        isChecked: boolean;

        onToggle: () => void;
    }

    interface ICheckBoxDynamic {
    }

    export class CheckBox extends React.Component<ICheckBoxStatic, ICheckBoxDynamic>{
        render(): JSX.Element {
            return <label className="vcrnet-checkbox vcrnet-toggleButton" data-checked={this.props.isChecked ? "yes" : null}>
                <input type="CHECKBOX" defaultChecked={this.props.isChecked} onClick={this.props.onToggle} />{this.props.children}
            </label>;
        }
    }
}
