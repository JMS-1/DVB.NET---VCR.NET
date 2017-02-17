/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IFavorite extends JMSLib.App.IConnectable {
        readonly title: string;

        readonly count: number;

        readonly remove: JMSLib.App.ICommand;

        readonly show: JMSLib.App.ICommand;
    }

    export interface IFavoritesPage extends IPage {
        readonly favorites: IFavorite[];

        readonly filter: JMSLib.App.IValueFromList<boolean>;
    }

    export class FavoritesPage extends Page implements IFavoritesPage {

        private static _filter: JMSLib.App.IUiValue<boolean>[] = [
            JMSLib.App.uiValue(true, "Alle Favoriten"),
            JMSLib.App.uiValue(false, "Nur Favoriten mit Sendungen"),
        ];

        private static _Entries: Favorites.Favorite[];

        readonly filter = new JMSLib.App.SelectSingleFromList<boolean>({ value: true }, "value", null, () => this.refreshUi(), true, FavoritesPage._filter);

        get favorites(): IFavorite[] {
            if (this.filter.value)
                return FavoritesPage._Entries;
            else
                return FavoritesPage._Entries.filter(e => e.count !== 0);
        }

        constructor(application: Application) {
            super(`favorites`, application);
        }

        reset(sections: string[]): void {
            Favorites.Favorite.resetLoader();

            if (!FavoritesPage._Entries)
                FavoritesPage._Entries = JSON.parse(this.application.profile.guideSearches || "[]").map(e => new Favorites.Favorite(e));

            var remove = this.remove.bind(this);
            var show = this.show.bind(this);

            FavoritesPage._Entries.forEach(f => f.registerRemove(show, remove));

            this.application.isBusy = false;
        }

        get title(): string {
            return `Gespeicherte Suchen`;
        }

        private show(favorite: Favorites.Favorite): void {
            this.application.guidePage.loadFilter(favorite.model);
        }

        private remove(favorite: Favorites.Favorite): JMSLib.App.IHttpPromise<void> {
            var favorites = FavoritesPage._Entries;

            return VCRServer.updateSearchQueries(favorites.map(f => f.model)).then(() => {
                FavoritesPage._Entries = favorites;

                this.refreshUi();
            })
        }
    }
}