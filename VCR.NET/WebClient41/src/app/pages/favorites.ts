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

        private _entries: Favorites.Favorite[];

        readonly filter = new JMSLib.App.SelectSingleFromList({ value: true }, "value", null, () => this.refreshUi(), FavoritesPage._filter)
            .addRequiredValidator();

        get favorites(): IFavorite[] {
            if (this.filter.value)
                return this.allFavorites;
            else
                return this.allFavorites.filter(e => e.count !== 0);
        }

        constructor(application: Application) {
            super(`favorites`, application);
        }

        private get allFavorites(): Favorites.Favorite[] {
            if (!this._entries)
                this._entries = JSON.parse(this.application.profile.guideSearches || "[]").map(e => new Favorites.Favorite(e));

            return this._entries;
        }

        reset(sections: string[]): void {
            this._entries = undefined;

            Favorites.Favorite.resetLoader();

            var remove = this.remove.bind(this);
            var show = this.show.bind(this);

            this.allFavorites.forEach(f => f.registerRemove(show, remove));

            this.application.isBusy = false;
        }

        get title(): string {
            return `Gespeicherte Suchen`;
        }

        private show(favorite: Favorites.Favorite): void {
            this.application.guidePage.loadFilter(favorite.model);
        }

        private remove(favorite: Favorites.Favorite): JMSLib.App.IHttpPromise<void> {
            var favorites = this.allFavorites.filter(f => f !== favorite);

            return VCRServer.updateSearchQueries(favorites.map(f => f.model)).then(() => {
                this._entries = favorites;

                this.refreshUi();
            })
        }

        add(favorite: VCRServer.SavedGuideQueryContract): JMSLib.App.IHttpPromise<void> {
            this._entries = undefined;

            return VCRServer
                .updateSearchQueries([favorite].concat(this.allFavorites.map(f => f.model)))
                .then(() => this.application.gotoPage(this.route));
        }
    }
}