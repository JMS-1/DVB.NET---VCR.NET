namespace VCRNETClient.App.NoUi {

    // Die äußere Sicht auf eine Seite der Anwendung.
    export interface IPage extends INoUiWithSite {
        // Rückgriff auf die Anwendung als Ganzes.
        readonly application: IApplication;

        // Dereindeutige Name (die Route) zur Seite.
        readonly route: string;

        // Die Überschrift der Seite.
        getTitle(): string;

        // Der Navigationsbereich.
        readonly navigation: {
            // Aktualisierung.
            readonly refresh: boolean;

            // Aufzeichnungsplan.
            readonly plan: boolean;

            // Programmzeitschrift.
            readonly guide: boolean;

            // Suchfavoriten.
            readonly favorites: boolean;

            // Neu anlegen.
            readonly new: boolean;

            // Laufende Aufzeichnungen.
            readonly current: boolean;
        };

        // Fordert zur Aktualisierung der zur Seite gehörenden Daten auf.
        reload(): void;
    }

    // Basisklasse zur Implementierung von Seiten.
    export abstract class Page<TSiteType extends INoUiSite> implements IPage {
        // Das zugehörige Oberflächenelement.
        private _site: TSiteType;

        // Legt das zugehörige Oberflächenelement fest.
        setSite(newSite: TSiteType): void {
            this._site = newSite;

            if (this._site)
                this.onSiteChanged();
        }

        // Meldet das zugehörige Oberflächenelement.
        protected getSite(): TSiteType {
            return this._site;
        }

        // Wird ausgelöst, wenn das zugehörige Oberflächenelement gesetzt wurde.
        protected onSiteChanged(): void {
        }

        // Meldet Änderungen an das zugehörige Oberflächenelement.
        protected refreshUi(): void {
            if (this._site)
                this._site.refreshUi();
        }

        // Initialisiert die Seite zur erneuten Anzeige.
        abstract reset(section: string): void;

        // Meldet die Überschrift der Seite.
        abstract getTitle(): string;

        // Initialisiert den Navigationsbereich.
        navigation = {
            favorites: false,
            refresh: false,
            current: true,
            guide: true,
            plan: true,
            new: true
        };

        // Initialisiert eine neue Seite.
        constructor(public readonly route: string, public readonly application: Application) {
        }

        // Aktualisiert den Inhalt der Seite.
        reload(): void {
        }
    }

}