/// <reference path="section.ts" />

namespace VCRNETClient.App.Admin {

    // Schnittstelle zur Pflege der erlaubten Aufzeichnungsverzeichnisse.
    export interface IAdminDirectoriesPage extends ISection {
        // Die aktuelle Liste der Aufzeichnungsverzeichnisse.
        readonly directories: JMSLib.App.IMultiValueFromList<string>;

        readonly share: JMSLib.App.IString;

        readonly showBrowse: boolean;

        readonly browse: JMSLib.App.IValueFromList<string>;

        readonly parent: JMSLib.App.ICommand;

        readonly add: JMSLib.App.ICommand;

        readonly remove: JMSLib.App.ICommand;

        readonly pattern: JMSLib.App.IString;
    }

    export class DirectoriesSection extends Section<VCRServer.DirectorySettingsContract> implements IAdminDirectoriesPage {

        readonly directories = new JMSLib.App.SelectMultipleFromList<string>({}, "value", null, () => this.refreshUi());

        readonly pattern = new JMSLib.App.String({}, "pattern", "Muster für Dateinamen", () => this.refreshUi(), true);

        readonly remove = new JMSLib.App.Command(() => this.removeDirectories(), "Verzeichnisse entfernen", () => this.directories.value.length > 0);

        private _shareValidation: string;

        readonly share = new JMSLib.App.String({}, "value", "Netzwerk-Share", () => this.onShareChanged(), null, null, str => this._shareValidation || ``);

        get showBrowse(): boolean {
            return (this.share.value || "").trim().length < 1;
        }

        readonly browse = new JMSLib.App.SelectSingleFromList<string>({}, "value", "Server-Verzeichnis", () => this.doBrowse());

        readonly parent = new JMSLib.App.Command(() => this.doBrowseUp(), "Übergeordnetes Verzeichnis", () => this.browse.value && this.showBrowse);

        readonly add = new JMSLib.App.Command(() => this.addDirectory(), "Verzeichnis hinzufügen", () => !!this.browse.value || !this.showBrowse);

        private _disableBrowse = false;

        protected loadAsync(): void {
            this.add.reset();
            this.remove.reset();
            this.parent.reset();

            VCRServer.getDirectorySettings().then(settings => this.initialize(settings));
        }

        private initialize(settings: VCRServer.DirectorySettingsContract): void {
            this.directories.allowedValues = settings.directories.map(d => JMSLib.App.uiValue(d));

            this.pattern.data = settings;

            VCRServer.browseDirectories(``, true).then(dirs => this.setDirectories(dirs));
        }

        private setDirectories(directories: string[]): void {
            this._disableBrowse = true;

            this.browse.allowedValues = (directories || []).map(d => JMSLib.App.uiValue(d, d || `<Bitte auswählen>`));
            this.browse.value = this.browse.allowedValues[0].value;

            this._disableBrowse = false;

            this.refreshUi();

            this.page.application.isBusy = false;
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
            this.directories.allowedValues = this.directories.allowedValues.filter(v => !v.isSelected);
        }

        private onShareChanged(): void {
            this.share.validate();

            this.refreshUi();
        }

        protected get isValid(): boolean {
            return this.pattern.message === ``;
        }

        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            var settings: VCRServer.DirectorySettingsContract = this.pattern.data;

            settings.directories = this.directories.allowedValues.map(v => v.value);

            return VCRServer.setDirectorySettings(settings);
        }

        private addDirectory(folder?: string): JMSLib.App.IHttpPromise<void> {
            if (folder) {
                folder = folder.trim();

                if (folder.length > 0)
                    if (folder[folder.length - 1] != `\\`)
                        folder += `\\`;

                if (!this.directories.allowedValues.some(v => v.value === folder))
                    this.directories.allowedValues = this.directories.allowedValues.concat([JMSLib.App.uiValue(folder)]);

                return null;
            }

            if (this.showBrowse) {
                var selected = this.browse.value;
                if (selected)
                    this.addDirectory(selected);

                return null;
            }

            var share = this.share.value.trim();

            this._shareValidation = undefined;
            this.share.validate();

            return VCRServer.validateDirectory(share).then(ok => {
                if (ok)
                    this.addDirectory(share);
                else {
                    this._shareValidation = "Ungültiges Verzeichnis";
                    this.share.validate();

                    this.refreshUi();
                }
            });
        }
    }
}