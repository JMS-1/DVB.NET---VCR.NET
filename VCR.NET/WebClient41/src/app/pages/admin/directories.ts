namespace VCRNETClient.App.Admin {

    export interface IAdminDirectoriesPage extends IAdminSection {
    }

    export class DirectoriesSection implements IAdminDirectoriesPage {

        constructor(public readonly page: AdminPage) {
        }

        reset(): void {
            VCRServer.getDirectorySettings().then(settings => this.setSettings(settings));
        }

        private setSettings(settings: VCRServer.DirectorySettingsContract): void {
            this.page.application.setBusy(false);
        }
    }
}