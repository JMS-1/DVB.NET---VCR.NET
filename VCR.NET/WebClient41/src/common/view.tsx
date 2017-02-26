/// <reference path="../lib.react/router.tsx" />

namespace VCRNETClient.Ui {

    // Konkretisierte React.Js Komponente zur Anzeige unterschiedlicher Navigationsbereiche.
    export class View extends JMSLib.ReactUi.Router<App.IPage> {

        // Anmeldung der Navigationsbereiche für die Basisklasse.
        protected getPages(page: App.IPage): JMSLib.ReactUi.IPageFactory<App.IPage> {
            return {
                [page.application.logPage.route]: Log,
                [page.application.jobPage.route]: Jobs,
                [page.application.homePage.route]: Home,
                [page.application.planPage.route]: Plan,
                [page.application.editPage.route]: Edit,
                [page.application.helpPage.route]: Help,
                [page.application.guidePage.route]: Guide,
                [page.application.adminPage.route]: Admin,
                [page.application.devicesPage.route]: Devices,
                [page.application.settingsPage.route]: Settings,
                [page.application.favoritesPage.route]: Favorites,
            };
        }
    }

}
