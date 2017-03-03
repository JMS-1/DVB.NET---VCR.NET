/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Anzeige und Pflege einer gespeicherten Suche.
    export class Favorite extends JMSLib.ReactUi.ComponentWithSite<App.Favorites.IFavorite> {

        // Oberflächenelemente anlegen.
        render(): JSX.Element {
            return <tr className="vcrnet-favorite">
                <td><JMSLib.ReactUi.InternalLink description="In der Programmzeitschrift anzeigen" view={() => this.props.uvm.show.execute()}>{(this.props.uvm.count === null) ? `-` : `${this.props.uvm.count}`}</JMSLib.ReactUi.InternalLink></td>
                <td>{this.props.uvm.title} <JMSLib.ReactUi.Pictogram onClick={() => this.props.uvm.remove.execute()} name="delete" /></td>
            </tr>;
        }
    }
}