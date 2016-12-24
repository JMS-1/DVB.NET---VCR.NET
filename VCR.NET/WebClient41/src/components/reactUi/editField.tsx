/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient {
    interface IFieldStatic {
        label: string;

        help?: string;
    }

    interface IFieldDynamic {
    }

    export class Field extends React.Component<IFieldStatic, IFieldDynamic>{
        render(): JSX.Element {
            return <label className="vcrnet-editfield">
                <div>{this.props.label}{this.props.help ? <HelpLink page={this.props.help} /> : null}</div>
                {this.props.children}
            </label>;
        }
    }
}
