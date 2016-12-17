namespace VCRNETClient.App {
    export interface IApplicationSite {
        onBusyChanged(isBusy: boolean): void;

        onFirstStart(): void;
    }

    export class Application {
        private _homePage = new HomePage(this);

        private _planPage = new PlanPage(this);

        // Nach aussen hin sichtbarer globaler Zustand.
        version: VCRServer.InfoServiceContract;

        profile: VCRServer.UserProfileContract;

        page: App.Page;

        // Initial sind wir gesperrt.
        private _busy = true;

        // Wieviele ausstehende Zugriffe bis zum Start gibt es noch.
        private _startPending = 2;

        constructor(private _site: IApplicationSite) {
            var testStart = this.testStart.bind(this);

            // Alle Startvorgänge einleiten.
            VCRServer.getServerVersion().then(info => this.version = info).then(testStart);
            VCRServer.getUserProfile().then(profile => this.profile = profile).then(testStart);
        }

        private onVersionAvailable(info: VCRServer.InfoServiceContract): void {
            this.version = info;
            this.testStart();
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
            switch (name) {
                case PlanPage.name:
                    this.page = this._planPage;
                    break;
                default:
                    this.page = this._homePage;
                    break;
            }

            // Zustand wie beim Erstaufruf vorbereiten.
            this.page.reset();

            return true;
        }

        setBusy(isBusy: boolean): void {
            if (isBusy !== this._busy)
                this._site.onBusyChanged(this._busy = isBusy);
        }
    }
}