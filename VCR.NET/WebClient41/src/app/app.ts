namespace VCRNETClient.App {
    export interface IApplicationSite {
        onNewServerVersion: (info: VCRServer.InfoServiceContract) => void;

        onNewPage: (page: App.Page) => void;

        onBusyChanged: (isBusy: boolean) => void;
    }

    export class Application {
        private _homePage = new HomePage(this);

        private _planPage = new PlanPage(this);

        // Initial sind wir gesperrt.
        private _busy = true;

        constructor(private _site: IApplicationSite) {
            // Alle Startvorgänge einleiten.
            VCRServer.getServerVersion().then(this.onVersionAvailable.bind(this));
        }

        private onVersionAvailable(info: VCRServer.InfoServiceContract): void {
            this._site.onNewServerVersion(info);

            // Alle Startvorgänge sind abgeschlossen, wir können nun die Standardseite aktivieren.
            this.setBusy(false);
            this.switchPage();
        }

        switchPage(name: string = HomePage.name, section: string = ""): void {
            // Wir sind noch in einem Übergang oder beim Starten
            if (this._busy)
                return;

            this.setBusy(true);

            // Den Singleton der gewünschten Seite ermitteln.
            var page: Page;

            switch (name) {
                case PlanPage.name:
                    page = this._planPage;
                    break;
                default:
                    page = this._homePage;
                    break;
            }

            // Zustand wie beim Erstaufruf vorbereiten.
            page.reset();

            this._site.onNewPage(page);
        }

        setBusy(isBusy: boolean): void {
            if (isBusy === this._busy)
                return;

            this._site.onBusyChanged(this._busy = isBusy);
        }
    }
}