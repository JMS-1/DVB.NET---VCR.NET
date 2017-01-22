/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient.Ui {
    interface IDetailRowStatic {
        prefixColumns?: number;

        dataColumns: number;
    }

    export class DetailRow extends React.Component<IDetailRowStatic, JMSLib.ReactUi.IEmpty>  {
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
