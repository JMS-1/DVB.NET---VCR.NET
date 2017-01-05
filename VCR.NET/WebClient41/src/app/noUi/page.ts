namespace VCRNETClient.App.NoUi {

    export interface IPage extends INoUiWithSite {
        getRoute(): string;

        getTitle(): string;

        showNavigation(): boolean;

        showRefresh(): boolean;

        showPlan(): boolean;

        showGuide(): boolean;

        showFavorites(): boolean;

        showNew(): boolean;

        showCurrent(): boolean;

        readonly reload: () => void;
    }

    export abstract class Page<TSiteType extends INoUiSite> implements IPage {
        private _site: TSiteType;

        setSite(newSite: TSiteType): void {
            this._site = newSite;

            if (this._site)
                this.onSiteChanged();
        }

        protected getSite(): TSiteType {
            return this._site;
        }

        protected onSiteChanged(): void {
        }

        protected refresh(): void {
            if (this._site)
                this._site.refresh();
        }

        abstract getRoute(): string;

        abstract reset(section: string): void;

        abstract getTitle(): string;

        readonly reload: () => void;

        constructor(protected readonly application: Application) {
            this.reload = this.onReload.bind(this);
        }

        showNavigation(): boolean {
            return true;
        }

        showRefresh(): boolean {
            return false;
        }

        showPlan(): boolean {
            return true;
        }

        showGuide(): boolean {
            return true;
        }

        showFavorites(): boolean {
            return false;
        }

        showNew(): boolean {
            return true;
        }

        showCurrent(): boolean {
            return true;
        }

        onReload(): void {
        }
    }
}