/// <reference path="../lib/router.tsx" />

namespace VCRNETClient.Ui {

    export class View extends JMSLib.ReactUi.Router<App.IPage> {
        protected getPages(page: App.IPage): JMSLib.ReactUi.IPageFactory<App.IPage> {
            return {
                [page.application.logPage.route]: Log,
                [page.application.jobPage.route]: Jobs,
                [page.application.homePage.route]: Home,
                [page.application.planPage.route]: Plan,
                [page.application.editPage.route]: Edit,
                [page.application.helpPage.route]: Help,
                [page.application.guidePage.route]: Guide,
            };
        }
    }

}
