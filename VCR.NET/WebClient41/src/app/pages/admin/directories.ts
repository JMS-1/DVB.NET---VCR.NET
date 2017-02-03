namespace VCRNETClient.App.Admin {

    export interface IAdminDirectoriesPage extends IAdminSection {
    }

    export class DirectoriesSection implements IAdminDirectoriesPage {

        constructor(public readonly page: AdminPage) {
        }

        reset(): void {
            this.page.application.setBusy(false);
        }
    }
}