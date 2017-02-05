/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminDirectoriesPage extends IAdminSection {
        readonly directories: JMSLib.App.IMultiValueFromList<string>;

        readonly share: JMSLib.App.IValidatedString;

        readonly showBrowse: boolean;

        readonly browse: JMSLib.App.IValidateStringFromList;

        readonly parent: JMSLib.App.ICommand;

        readonly add: JMSLib.App.ICommand;

        readonly remove: JMSLib.App.ICommand;

        readonly pattern: JMSLib.App.IValidatedString;

        readonly update: JMSLib.App.ICommand;
    }

    export class DirectoriesSection extends AdminSection implements IAdminDirectoriesPage {

        readonly directories = new JMSLib.App.SelectFromList<string>({ value: [] }, "value", () => this.refreshUi(), null, []);

        readonly pattern = new JMSLib.App.EditString({}, "pattern", () => this.refreshUi(), "Muster für Dateinamen", true);

        readonly remove = new JMSLib.App.Command(() => this.removeDirectories(), "Verzeichnisse entfernen", () => this.directories.value.length > 0);

        readonly update = new JMSLib.App.Command(() => this.save(), "Ändern", () => this.pattern.message === "");

        readonly share = new JMSLib.App.EditString({}, "value", () => this.onShareChanged(), "Netzwerk-Share", false);

        get showBrowse(): boolean {
            return (this.share.value || "").trim().length < 1;
        }

        readonly browse = new JMSLib.App.EditStringFromList({}, "value", () => this.doBrowse(), "Server-Verzeichnis", false, []);

        readonly parent = new JMSLib.App.Command(() => this.doBrowseUp(), "Übergeordnetes Verzeichnis", () => this.showBrowse && !!this.browse.value);

        readonly add = new JMSLib.App.Command(() => this.addDirectory(), "Verzeichnis hinzufügen", () => !this.showBrowse || !!this.browse.value);

        private _disableBrowse = false;

        reset(): void {
            VCRServer.getDirectorySettings().then(settings => this.setSettings(settings));
        }

        private setSettings(settings: VCRServer.DirectorySettingsContract): void {
            this.directories.values = settings.directories.map(d => <JMSLib.App.IUiValue<string>>{ display: d, value: d });
            this.directories.value = [];

            this.pattern.data = settings;

            VCRServer.browseDirectories(``, true).then(dirs => this.setDirectories(dirs));
        }

        private setDirectories(directories: string[]): void {
            this._disableBrowse = true;

            this.browse.allowedValues = (directories || []).map(d => <JMSLib.App.IUiValue<string>>{ value: d, display: d || `<Bitte auswählen>` });
            this.browse.value = this.browse.allowedValues[0].value;

            this._disableBrowse = false;

            this.refreshUi();

            this.page.application.setBusy(false);
        }

        private doBrowse(): void {
            if (this._disableBrowse)
                return;

            var folder = this.browse.value;
            if (folder)
                VCRServer.browseDirectories(folder, true).then(dirs => this.setDirectories(dirs));
        }

        private doBrowseUp(): void {
            var folder = this.browse.allowedValues[0].value;
            if (folder)
                VCRServer.browseDirectories(folder, false).then(dirs => this.setDirectories(dirs));
        }

        private removeDirectories(): void {
            var selected = {};

            this.directories.value.forEach(v => selected[v] = true);

            this.directories.values = this.directories.values.filter(v => !selected[v.value]);
            this.directories.value = [];
        }

        private onShareChanged(): void {
            this.share.validate();

            this.refreshUi();
        }

        refreshUi(): void {
            this.pattern.validate();

            super.refreshUi();
        }

        private save(): JMSLib.App.IHttpPromise<void> {
            var settings: VCRServer.DirectorySettingsContract = this.pattern.data;

            settings.directories = this.directories.allValues;

            return this.page.update(VCRServer.setDirectorySettings(settings));
        }

        private addDirectory(folder?: string): JMSLib.App.IHttpPromise<void> {
            if (folder) {
                folder = folder.trim();

                if (folder.length > 0)
                    if (folder[folder.length - 1] != `\\`)
                        folder += `\\`;

                if (!this.directories.values.some(v => v.value === folder)) {
                    this.directories.values.push({ display: folder, value: folder });

                    this.refreshUi();
                }

                return null;
            }

            if (this.showBrowse) {
                var selected = this.browse.value;
                if (selected)
                    this.addDirectory(selected);

                return null;
            }

            var share = this.share.value.trim();

            return VCRServer.validateDirectory(share).then(ok => {
                if (ok)
                    this.addDirectory(share);
                else {
                    this.share.message = "Ungültiges Verzeichnis";

                    this.refreshUi();
                }
            });
        }
    }
}