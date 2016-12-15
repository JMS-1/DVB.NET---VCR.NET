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

        page: App.Page;

        // Initial sind wir gesperrt.
        private _busy = true;

        constructor(private _site: IApplicationSite) {
            // Alle Startvorgänge einleiten.
            VCRServer.getServerVersion().done(this.onVersionAvailable.bind(this));
        }

        private onVersionAvailable(info: VCRServer.InfoServiceContract): void {
            this.version = info;

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