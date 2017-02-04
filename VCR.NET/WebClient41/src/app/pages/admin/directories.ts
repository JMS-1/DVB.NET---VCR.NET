/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminDirectoriesPage extends IAdminSection {
        readonly directories: JMSLib.App.IMultiValueFromList<string>;

        readonly pattern: JMSLib.App.IValidatedString;
    }

    export class DirectoriesSection extends AdminSection implements IAdminDirectoriesPage {

        readonly directories = new JMSLib.App.SelectFromList<string>({ value: [] }, "value", () => this.refreshUi(), null, []);

        readonly pattern = new JMSLib.App.EditString({}, "pattern", null, "Muster für Dateinamen", true);

        reset(): void {
            VCRServer.getDirectorySettings().then(settings => this.setSettings(settings));
        }

        private setSettings(settings: VCRServer.DirectorySettingsContract): void {
            this.directories.values = settings.directories.map(d => <JMSLib.App.IUiValue<string>>{ display: d, value: d });
            this.directories.value = [];

            this.pattern.data = settings;

            this.page.application.setBusy(false);
        }
    }
}