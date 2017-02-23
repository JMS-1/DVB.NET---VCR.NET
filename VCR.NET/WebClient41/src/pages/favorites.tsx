/// <reference path="../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Favorites extends JMSLib.ReactUi.ComponentWithSite<App.IFavoritesPage> {
        render(): JSX.Element {
            return <div className="vcrnet-favorites">
                {this.getHelp()}
                <JMSLib.ReactUi.SingleSelectButton uvm={this.props.uvm.filter} />
                <table>
                    <thead>
                        <tr>
                            <td>Sendungen</td>
                            <td>Suchbedingung</td>
                        </tr>
                    </thead>
                    <tbody>
                        {this.props.uvm.favorites.map((f, index) => <Favorite key={index} uvm={f} />)}
                    </tbody>
                </table>
            </div >;
        }

        private getHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Die Auswertung der passenden Sendungen erfolgt pro Favorit einmalig und wird verzögert im Hintergrund
                ausgeführt. Solange diese Berechnung noch nicht abgeschlossen ist, wird als Anzahl der Sendungen
                in der ersten Spalte ein Bindestrich
                dargestellt. Unabhängig davon ist es durch Auswahl der Anzahl jederzeit möglich, den Favoriten
                als Suche in der Programmzeitschrift anzuzeigen.
                <br />
                <br />
                Ein Favorit kann durch das Symbol <JMSLib.ReactUi.Pictogram name="delete" /> rechts
                neben der Beschreibung der Suchbedingung jederzeit gelöscht werden. Dieser
                Vorgang wird unmittelbar und unwiederbringlich ohne weitere Rückfrage ausgeführt.
            </InlineHelp>;
        }
    }

}
