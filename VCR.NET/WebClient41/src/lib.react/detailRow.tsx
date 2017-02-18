namespace JMSLib.ReactUi {
    interface IDetailRow {
        prefixColumns?: number;

        dataColumns: number;
    }

    export class DetailRow extends React.Component<IDetailRow, IEmpty>  {
        render(): JSX.Element {
            return <tr className="jmslib-details">
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
