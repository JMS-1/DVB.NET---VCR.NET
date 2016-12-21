namespace VCRNETClient.App {
    export interface IApplicationSite {
        onBusyChanged(isBusy: boolean): void;

        onFirstStart(): void;
    }

    export class Application {
        private _homePage = new HomePage(this);

        private _pageMapper: { [name: string]: Page } = {};

        // Nach aussen hin sichtbarer globaler Zustand.
        version: VCRServer.InfoServiceContract;

        profile: VCRServer.UserProfileContract;

        page: App.Page;

        // Initial sind wir gesperrt.
        private _busy = true;

        // Wieviele ausstehende Zugriffe bis zum Start gibt es noch.
        private _startPending = 2;

        constructor(private _site: IApplicationSite) {
            // Alle bekannten Seiten.
            var pages: Page[] = [this._homePage, new PlanPage(this), new EditPage(this)];

            // Abbildung erstellen.
            pages.forEach(p => this._pageMapper[p.getName()] = p);

            var testStart = this.testStart.bind(this);

            // Alle Startvorgänge einleiten.
            VCRServer.getServerVersion().then(info => this.version = info).then(testStart);
            VCRServer.getUserProfile().then(profile => this.profile = profile).then(testStart);
        }

        private testStart(): void {
            if (this._startPending-- > 1)
                return;

            // Alle Startvorgänge sind abgeschlossen
            this.setBusy(false);

            // Wir können nun die Standardseite aktivieren
            this._site.onFirstStart();
        }

        switchPage(name: string, section: string): boolean {
            // Wir sind noch in einem Übergang oder beim Starten
            if (this._busy)
                return false;

            this.setBusy(true);

            // Den Singleton der gewünschten Seite ermitteln.
            this.page = this._pageMapper[name] || this._homePage;

            // Zustand wie beim Erstaufruf vorbereiten.
            this.page.reset(section);

            return true;
        }

        setBusy(isBusy: boolean): void {
            if (isBusy !== this._busy)
                this._site.onBusyChanged(this._busy = isBusy);
        }

        getTitle(): string {
            var title = "VCR.NET Recording Service";
            var version = this.version;

            if (version)
                return `${title} ${version.version}`;
            else
                return title;
        }
    }
}