/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminDirectoriesPage extends IAdminSection {
        readonly pattern: JMSLib.App.IValidatedString;
    }

    export class DirectoriesSection extends AdminSection implements IAdminDirectoriesPage {

        readonly pattern = new JMSLib.App.EditString({}, "pattern", null, "Muster für Dateinamen", true);

        reset(): void {
            VCRServer.getDirectorySettings().then(settings => this.setSettings(settings));
        }

        private setSettings(settings: VCRServer.DirectorySettingsContract): void {
            this.pattern.data = settings;

            this.page.application.setBusy(false);
        }
    }
}