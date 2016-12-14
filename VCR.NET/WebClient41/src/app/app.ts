namespace VCRNETClient.App {
    export interface IApplicationSite {
        onNewServerVersion: (info: VCRServer.InfoServiceContract) => void;
    }

    export class Application {
        homePage = new HomePage(this);

        planPage = new PlanPage(this);

        onNewPage: (page: Page) => void;

        constructor(site: IApplicationSite) {
            VCRServer.getServerVersion().then(info => site.onNewServerVersion(info));
        }

        setPage(name: string = HomePage.name, section: string = ""): void {
            var page: Page;

            switch (name) {
                case PlanPage.name:
                    page = this.planPage;
                    break;
                default:
                    page = this.homePage;
                    break;
            }

            if (this.onNewPage)
                this.onNewPage(page);
        }
    }
}