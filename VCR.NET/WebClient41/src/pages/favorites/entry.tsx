/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Favorite extends JMSLib.ReactUi.ComponentWithSite<App.IFavorite> {
        render(): JSX.Element {
            return <tr className="vcrnet-favorite">
                <td>{(this.props.noui.count === null) ? `-` : `${this.props.noui.count}`}</td>
                <td>{this.props.noui.title} <JMSLib.ReactUi.Pictogram onClick={() => this.props.noui.remove.execute()} name="delete" /></td>
            </tr>;
        }
    }
}