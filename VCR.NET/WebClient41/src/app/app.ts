namespace VCRNETClient.App {
    export class Application {
        private _homePage: HomePage;

        getHomePage(): HomePage {
            if (!this._homePage)
                this._homePage = new HomePage(this);

            return this._homePage;
        }

        private _planPage: PlanPage;

        getPlanPage(): PlanPage {
            if (!this._planPage)
                this._planPage = new PlanPage(this);

            return this._planPage;
        }

        private _serverVersion: VCRServer.InfoServiceContract;

        onNewServerVersion: (info: VCRServer.InfoServiceContract) => void;

        onNewPage: (page: Page) => void;

        constructor() {
            VCRServer.getServerVersion().then(this.setServerInfo.bind(this));
        }

        private setServerInfo(info: VCRServer.InfoServiceContract): void {
            this._serverVersion = info;

            if (this.onNewServerVersion)
                this.onNewServerVersion(info);
        }

        setPage(name: string = HomePage.name, section: string = ""): void {
            var page: Page;

            switch (name) {
                case PlanPage.name:
                    page = this.getPlanPage();
                    break;
                default:
                    page = this.getHomePage();
                    break;
            }

            if (this.onNewPage)
                this.onNewPage(page);
        }
    }
}