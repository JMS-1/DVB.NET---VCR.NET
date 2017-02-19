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

        private ensureFavorites(): Favorites.Favorite[] {
            if (!FavoritesPage._Entries)
                FavoritesPage._Entries = JSON.parse(this.application.profile.guideSearches || "[]").map(e => new Favorites.Favorite(e));

            return FavoritesPage._Entries;
        }

        reset(sections: string[]): void {
            Favorites.Favorite.resetLoader();

            var remove = this.remove.bind(this);
            var show = this.show.bind(this);

            this.ensureFavorites().forEach(f => f.registerRemove(show, remove));

            this.application.isBusy = false;
        }

        get title(): string {
            return `Gespeicherte Suchen`;
        }

        private show(favorite: Favorites.Favorite): void {
            this.application.guidePage.loadFilter(favorite.model);
        }

        private remove(favorite: Favorites.Favorite): JMSLib.App.IHttpPromise<void> {
            var favorites = FavoritesPage._Entries.filter(f => f !== favorite);

            return VCRServer.updateSearchQueries(favorites.map(f => f.model)).then(() => {
                FavoritesPage._Entries = favorites;

                this.refreshUi();
            })
        }

        add(favorite: VCRServer.SavedGuideQueryContract): JMSLib.App.IHttpPromise<void> {
            var favorites = [new Favorites.Favorite(favorite)].concat(this.ensureFavorites());

            return VCRServer.updateSearchQueries(favorites.map(f => f.model)).then(() => {
                FavoritesPage._Entries = favorites;

                this.application.gotoPage(this.route);
            });
        }
    }
}