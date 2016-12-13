namespace VCRNETClient.App {
    export class Application {
        private _currentPage: Page;

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
            switch (name) {
                case PlanPage.name:
                    this._currentPage = new PlanPage(this);
                    break;
                default:
                    this._currentPage = new HomePage(this);
                    break;
            }

            if (this.onNewPage)
                this.onNewPage(this._currentPage);
        }
    }
}