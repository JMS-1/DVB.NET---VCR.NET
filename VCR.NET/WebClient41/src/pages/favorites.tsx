/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Favorites extends JMSLib.ReactUi.ComponentWithSite<App.IFavoritesPage> {
        render(): JSX.Element {
            return <div className="vcrnet-favorites">
                <JMSLib.ReactUi.ButtonFromList noui={this.props.noui.filter} />
                <table>
                    <thead>
                        <tr>
                            <td>Sendungen</td>
                            <td>Suchbedingung</td>
                        </tr>
                    </thead>
                    <tbody>
                        {this.props.noui.favorites.map((f, index) => <Favorite key={index} noui={f} />)}
                    </tbody>
                </table>
            </div >;
        }
    }

}
