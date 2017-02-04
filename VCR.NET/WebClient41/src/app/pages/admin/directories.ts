/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminDirectoriesPage extends IAdminSection {
        readonly directories: JMSLib.App.IMultiValueFromList<string>;

        readonly remove: JMSLib.App.ICommand;

        readonly pattern: JMSLib.App.IValidatedString;
    }

    export class DirectoriesSection extends AdminSection implements IAdminDirectoriesPage {

        readonly directories = new JMSLib.App.SelectFromList<string>({ value: [] }, "value", () => this.refreshUi(), null, []);

        readonly pattern = new JMSLib.App.EditString({}, "pattern", null, "Muster für Dateinamen", true);

        readonly remove = new JMSLib.App.Command(() => this.removeDirectories(), "Verzeichnisse entfernen", () => this.directories.value.length > 0);

        reset(): void {
            VCRServer.getDirectorySettings().then(settings => this.setSettings(settings));
        }

        private setSettings(settings: VCRServer.DirectorySettingsContract): void {
            this.directories.values = settings.directories.map(d => <JMSLib.App.IUiValue<string>>{ display: d, value: d });
            this.directories.value = [];

            this.pattern.data = settings;

            this.page.application.setBusy(false);
        }

        private removeDirectories(): void {
            var selected = {};

            this.directories.value.forEach(v => selected[v] = true);

            this.directories.values = this.directories.values.filter(v => !selected[v.value]);
            this.directories.value = [];
        }
    }
}