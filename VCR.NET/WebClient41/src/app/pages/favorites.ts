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

    class Favorite implements IFavorite {

        private static _loader: JMSLib.App.IHttpPromise<void>;

        static resetLoader(): void {
            Favorite._loader = new JMSLib.App.Promise<void, JMSLib.App.IHttpErrorInformation>(success => success(undefined));
        }

        constructor(public readonly model: VCRServer.SavedGuideQueryContract) {
        }

        site: JMSLib.App.ISite;

        readonly remove = new JMSLib.App.Command(() => this._remove(this), "Löschen");

        readonly show = new JMSLib.App.Command(() => this._show(this), "Anzeigen");

        private _remove: (favorite: Favorite) => JMSLib.App.IHttpPromise<void>;

        private _show: (favorite: Favorite) => void;

        registerRemove(show: (favorite: Favorite) => void, remove: (favorite: Favorite) => JMSLib.App.IHttpPromise<void>): void {
            this._remove = remove;
            this._show = show;

            this.remove.reset();
            this.show.reset();
        }

        get title(): string {
            var display = 'Alle ';

            if ((this.model.source || '') == '') {
                if (this.model.encryption == VCRServer.GuideEncryption.FREE)
                    display += 'unverschlüsselten ';
                else if (this.model.encryption == VCRServer.GuideEncryption.PAY)
                    display += 'verschlüsselten ';

                if (this.model.sourceType == VCRServer.GuideSource.TV)
                    display += 'Fernseh-';
                else if (this.model.sourceType == VCRServer.GuideSource.RADIO)
                    display += 'Radio-';
            }

            display += 'Sendungen, die über das Gerät ';
            display += this.model.device;

            if (this.model.source != null)
                if (this.model.source.length > 0) {
                    display += ' von der Quelle ';
                    display += this.model.source;
                }

            display += ' empfangen werden und deren Name ';
            if (!this.model.titleOnly)
                display += 'oder Beschreibung ';

            display += ' "';
            display += this.model.text.substr(1);
            display += '" ';

            if (this.model.text[0] == '*')
                display += 'enthält';
            else
                display += 'ist';

            return display;
        }

        private _count: number = null;

        get count(): number {
            if (this._count !== null)
                return this._count;

            var filter: VCRServer.GuideFilterContract = {
                content: this.model.titleOnly ? undefined : this.model.text,
                cryptFilter: this.model.encryption,
                typeFilter: this.model.sourceType,
                station: this.model.source,
                device: this.model.device,
                title: this.model.text,
                start: null,
                index: 0,
                size: 0,
            };

            Favorite._loader = Favorite._loader.then(() =>
                VCRServer.countProgramGuide(filter).then(count => {
                    this._count = count;

                    if (this.site)
                        this.site.refreshUi();
                }));

            return null;
        }
    }

    export class FavoritesPage extends Page implements IFavoritesPage {

        private static _filter: JMSLib.App.IUiValue<boolean>[] = [
            JMSLib.App.uiValue(true, "Alle Favoriten"),
            JMSLib.App.uiValue(false, "Nur Favoriten mit Sendungen"),
        ];

        private static _Entries: Favorite[];

        readonly filter = new JMSLib.App.SelectSingleFromList<boolean>({ value: true }, "value", null, () => this.refreshUi(), true, FavoritesPage._filter);

        get favorites(): Favorite[] {
            if (this.filter.value)
                return FavoritesPage._Entries;
            else
                return FavoritesPage._Entries.filter(e => e.count !== 0);
        }

        constructor(application: Application) {
            super(`favorites`, application);
        }

        reset(sections: string[]): void {
            Favorite.resetLoader();

            if (!FavoritesPage._Entries)
                FavoritesPage._Entries = JSON.parse(this.application.profile.guideSearches || "[]").map(e => new Favorite(e));

            var remove = this.remove.bind(this);
            var show = this.show.bind(this);

            FavoritesPage._Entries.forEach(f => f.registerRemove(show, remove));

            this.application.isBusy = false;
        }

        get title(): string {
            return `Gespeicherte Suchen`;
        }

        private show(favorite: Favorite): void {
            this.application.guidePage.loadFilter(favorite.model);
        }

        private remove(favorite: Favorite): JMSLib.App.IHttpPromise<void> {
            var favorites = FavoritesPage._Entries;

            return VCRServer.updateSearchQueries(favorites.map(f => f.model)).then(() => {
                FavoritesPage._Entries = favorites;

                this.refreshUi();
            })
        }
    }
}