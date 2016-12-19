/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IDetailRowStatic {
        prefixColumns?: number;

        dataColumns: number;
    }

    interface IDetailRowDynamic {
    }

    export class DetailRow extends React.Component<IDetailRowStatic, IDetailRowDynamic>  {
        render(): JSX.Element {
            return <tr className="vcrnet-details">
                {this.props.prefixColumns ? <td colSpan={this.props.prefixColumns}>
                    &nbsp;
                </td> : null}
                <td colSpan={this.props.dataColumns}>
                    {this.props.children}
                </td>
            </tr>;
        }
    }
}
