/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Favorite extends JMSLib.ReactUi.ComponentWithSite<App.Favorites.IFavorite> {
        render(): JSX.Element {
            return <tr className="vcrnet-favorite">
                <td><a href="javascript:void(0)" title="In der Programmzeitschrift anzeigen" onClick={() => this.props.uvm.show.execute()}>{(this.props.uvm.count === null) ? `-` : `${this.props.uvm.count}`}</a></td>
                <td>{this.props.uvm.title} <JMSLib.ReactUi.Pictogram onClick={() => this.props.uvm.remove.execute()} name="delete" /></td>
            </tr>;
        }
    }
}