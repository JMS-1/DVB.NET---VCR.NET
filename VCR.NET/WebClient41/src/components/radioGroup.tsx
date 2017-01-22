/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IRadioGroupStatic {
    }

    export class RadioGroup extends React.Component<IRadioGroupStatic, JMSLib.ReactUi.INoDynamicState>{
        render(): JSX.Element {
            return <div className="vcrnet-radioGroup">{this.props.children}</div>
        }
    }
}
