/// <reference path="page.ts" />

namespace VCRNETClient.App {

    // Schnittstelle zur Pflege der gespeicherten Suchen.
    export interface IFavoritesPage extends IPage {
        // Alle gespeicherten Suchen.
        readonly favorites: Favorites.IFavorite[];

        // Erlaubt die Einschränklung der Anzeige auf nur die Suchen, zu denen aktuell auch Sendungen in der Programmzeitschrift existieren.
        readonly onlyWithCount: JMSLib.App.IValueFromList<boolean>;
    }

    // Präsentationsmodell zur Anzeige und Pflege der gespeicherten Suchen.
    export class FavoritesPage extends Page implements IFavoritesPage {

        // Die Einstellungen des Filters.
        private static _filter: JMSLib.App.IUiValue<boolean>[] = [
            JMSLib.App.uiValue(true, "Alle Favoriten"),
            JMSLib.App.uiValue(false, "Nur Favoriten mit Sendungen"),
        ];

        // Erlaubt die Einschränklung der Anzeige auf nur die Suchen, zu denen aktuell auch Sendungen in der Programmzeitschrift existieren.
        readonly onlyWithCount = new JMSLib.App.SelectSingleFromList({ value: true }, "value", null, () => this.refreshUi(), FavoritesPage._filter);

        // Alle gespeicherten Suchen.
        private _entries: Favorites.Favorite[];

        get favorites(): Favorites.IFavorite[] {
            // Eventuell wollen wir nur die Favoriten sehen, zu denen es auch Daten gibt.
            if (this.onlyWithCount.value)
                return this._entries;
            else
                return this._entries.filter(e => e.count !== 0);
        }

        // Erstellt ein neues Präsentationsmodell.
        constructor(application: Application) {
            super(`favorites`, application);
        }

        // Ermittelt die Liste der gespeicherten Suche neu - dabei wird auch eine neue Anfrage der Anzahl der Sender gestellt.
        private readFavorites(): Favorites.Favorite[] {
            return JSON.parse(this.application.profile.guideSearches || "[]").map(e => new Favorites.Favorite(e, f => this.show(f), f => this.remove(f), () => this.refreshUi()));
        }

        // Bereitet die Anzeige des Präsentationsmodells vor.
        reset(sections: string[]): void {
            // Neue Sequenz von Ladevorgängen aufsetzen.
            Favorites.Favorite.resetLoader();

            // Die Liste der Favoriten neu aus der Konfiguration erstellen.
            this._entries = this.readFavorites();

            // Die Anwendung zur Bedienung freigeben.
            this.application.isBusy = false;
        }

        // Der Titel des Präsentationsmodells.
        get title(): string {
            return `Gespeicherte Suchen`;
        }

        // Zeigt das Ergebnis einer gespeicherten Suche in der Programmzeitschrift an.
        private show(favorite: Favorites.Favorite): void {
            this.application.guidePage.loadFilter(favorite.model);
        }

        // Entfernt eine gespeicherte Suche aus der Liste der gespeicherten Suchen.
        private remove(favorite: Favorites.Favorite): Promise<void> {
            var favorites = this._entries.filter(f => f !== favorite);

            // Dazu müssen wir uns eine asynchrone Bestätigung vom VCR.NET Recording Service holen.
            return VCRServer.updateSearchQueries(favorites.map(f => f.model)).then(() => {
                // Liste aktualisieren.
                this._entries = favorites;

                // Oberfläche zur Aktualisierung auffordern.
                this.refreshUi();
            })
        }

        // Ergänzt einen Favoritien.
        add(favorite: VCRServer.SavedGuideQueryContract): Promise<void> {
            // Ist der Aufruf erfolgreich so wechseln wir zur Ansicht der Favoriten - üblicherweise kommen wir aus der Programmzeitschrift.
            return VCRServer
                .updateSearchQueries([favorite].concat(this.readFavorites().map(f => f.model)))
                .then(() => this.application.gotoPage(this.route));
        }
    }
}