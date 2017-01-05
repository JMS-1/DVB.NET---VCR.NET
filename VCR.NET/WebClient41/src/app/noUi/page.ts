namespace VCRNETClient.App.NoUi {

    // Die äußere Sicht auf eine Seite der Anwendung.
    export interface IPage extends INoUiWithSite {
        // Die Überschrift der Seite.
        getTitle(): string;

        // Der Navigationsbereich.
        readonly navigation: {
            // Aktualisierung.
            refresh: boolean;

            // Aufzeichnungsplan.
            plan: boolean;

            // Programmzeitschrift.
            guide: boolean;

            // Suchfavoriten.
            favorites: boolean;

            // Neu anlegen.
            new: boolean;

            // Laufende Aufzeichnungen.
            current: boolean;
        };

        // Fordert zur Aktualisierung der zur Seite gehörenden Daten auf.
        reload(): void;
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

        navigation = {
            favorites: false,
            refresh: false,
            current: true,
            guide: true,
            plan: true,
            new: true
        };

        constructor(protected readonly application: Application) {
        }

        reload(): void {
        }
    }
}