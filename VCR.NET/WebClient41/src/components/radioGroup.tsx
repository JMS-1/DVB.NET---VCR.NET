/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IRadioGroupStatic {
    }

    interface IRadioGroupDynamic {
    }

    export class RadioGroup extends React.Component<IRadioGroupStatic, IRadioGroupDynamic>{
        render(): JSX.Element {
            return <div className="vcrnet-radioGroup">{this.props.children}</div>
        }
    }
}
