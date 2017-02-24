/// <reference path="section.ts" />

namespace VCRNETClient.App.Admin {

    // Schnittstelle zur Pflege der erlaubten Aufzeichnungsverzeichnisse.
    export interface IAdminDirectoriesPage extends ISection {
        // Die aktuelle Liste der Aufzeichnungsverzeichnisse.
        readonly directories: JMSLib.App.IMultiValueFromList<string>;

        // Eingabe eines Netzwerklaufwerks.
        readonly share: JMSLib.App.IString;

        // Meldet, ob die verzeichnisauswahl angezeigt werden soll.
        readonly showBrowse: boolean;

        // Die aktuelle Verzeichnisauswahl.
        readonly browse: JMSLib.App.IValueFromList<string>;

        // Befehl um in der Verzeichnisauswahl zum übergeordneten Verzeichnis zu wechseln.
        readonly parent: JMSLib.App.ICommand;

        // Befehl zur Eintragung des netzwerklaufwerks (nach Prüfung) oder des ausgewählten Verzeichnisses in die Verzeichnisliste.
        readonly add: JMSLib.App.ICommand;

        // Befehl zum Entfernen der ausgewählten Verzeichnisse aus der Verzeichnisliste.
        readonly remove: JMSLib.App.ICommand;

        // Das aktuelle Muster für die Namen von Aufzeichnungsdateien.
        readonly pattern: JMSLib.App.IString;
    }

    // Präsentationsmodell zur Pflege der Konfiguration der Aufzeichnungsverzeichnisse.
    export class DirectoriesSection extends Section implements IAdminDirectoriesPage {

        // Die aktuelle Liste der Aufzeichnungsverzeichnisse.
        readonly directories = new JMSLib.App.SelectMultipleFromList<string>({}, "value", null, () => this.refreshUi());

        // Das aktuelle Muster für die Namen von Aufzeichnungsdateien.
        readonly pattern = new JMSLib.App.String({}, "pattern", "Muster für Dateinamen", () => this.refreshUi(), true);

        // Befehl zum Entfernen der ausgewählten Verzeichnisse aus der Verzeichnisliste.
        readonly remove = new JMSLib.App.Command(() => this.removeDirectories(), "Verzeichnisse entfernen", () => this.directories.value.length > 0);

        // Fehlermeldung zur letzten Pfüfung des Netzwerkverzeichnisses.
        private _shareValidation: string;

        // Eingabe eines Netzwerklaufwerks.
        readonly share = new JMSLib.App.String({}, "value", "Netzwerk-Share", () => this.refreshUi(), null, null, str => this._shareValidation || ``);

        // Gesetzt wenn die Verzeichnisauswahl angezeigt werden soll.
        get showBrowse(): boolean {
            return (this.share.value || "").trim().length < 1;
        }

        // Die aktuelle Verzeichnisauswahl.
        readonly browse = new JMSLib.App.SelectSingleFromList<string>({}, "value", "Server-Verzeichnis", () => this.doBrowse());

        // Befehl um in der Verzeichnisauswahl zum übergeordneten Verzeichnis zu wechseln.
        readonly parent = new JMSLib.App.Command(() => this.doBrowseUp(), "Übergeordnetes Verzeichnis", () => this.browse.value && this.showBrowse);

        // Befehl zum Entfernen der ausgewählten Verzeichnisse aus der Verzeichnisliste.
        readonly add = new JMSLib.App.Command(() => this.onAdd(), "Verzeichnis hinzufügen", () => !!this.browse.value || !this.showBrowse);

        // Gesetzt während die Liste der Verzeichnisse aktualisiert wird.
        private _disableBrowse = false;

        // Fordert die Konfiguration an.
        protected loadAsync(): void {
            this.add.reset();
            this.remove.reset();
            this.parent.reset();

            this._shareValidation = undefined;
            this.share.value = null;

            // Konfiguration anfordern.
            VCRServer.getDirectorySettings().then(settings => {
                // Liste der erlaubten Verzeichnisse laden.
                this.directories.allowedValues = settings.directories.map(d => JMSLib.App.uiValue(d));

                // Pflege des Dateinamenmusters vorbereiten.
                this.pattern.data = settings;

                // Wurzelverzeichnisse laden.
                VCRServer.browseDirectories(``, true).then(dirs => this.setDirectories(dirs));
            });
        }

        // Aktualisiert die Liste der auswählbaren Verzeichnisse.
        private setDirectories(directories: string[]): void {
            // Aktualisierungen vermeiden.
            this._disableBrowse = true;

            // Auswahlliste vorbereiten und mit dem ersten Verzeichnis initialisieren.
            this.browse.allowedValues = (directories || []).map(d => JMSLib.App.uiValue(d, d || `<Bitte auswählen>`));
            this.browse.value = this.browse.allowedValues[0].value;

            // Alles wie wie üblich.
            this._disableBrowse = false;

            // Nun müssen wir die Aktualisierung der Oberfläche anfordern.
            this.refreshUi();

            // Beim ersten Aufruf können wir die Anwendung nun zur Bedienung freigeben.
            this.page.application.isBusy = false;
        }

        // Die Auswahl des Verzeichnisses hat sich verändert.
        private doBrowse(): void {
            // Wir laden gerade die Liste.
            if (this._disableBrowse)
                return;

            // Alle Unterverzeichnisse ermitteln.
            var folder = this.browse.value;
            if (folder)
                VCRServer.browseDirectories(folder, true).then(dirs => this.setDirectories(dirs));
        }

        // Das übergeordnete Verzeichnis soll angezeigt werden.
        private doBrowseUp(): void {
            // In eine höhere Ansicht wechseln.
            var folder = this.browse.allowedValues[0].value;
            if (folder)
                VCRServer.browseDirectories(folder, false).then(dirs => this.setDirectories(dirs));
        }

        // Ausgewählte Verzeichnisse aus der Liste entfernen.
        private removeDirectories(): void {
            this.directories.allowedValues = this.directories.allowedValues.filter(v => !v.isSelected);
        }

        // Wenn das Dateimuster gültig ist, kann die Konfiguration abgespeichert werden - selbst eine leere Verzeichnisliste ist in Ordnung.
        protected get isValid(): boolean {
            return this.pattern.message === ``;
        }

        // Sendet die veränderte Konfiguration an den VCR.NET Recording Service.
        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            // Die aktuell erlaubten Verzeichnisse werden als Verzeichnisliste übernommen.
            var settings: VCRServer.DirectorySettingsContract = this.pattern.data;

            settings.directories = this.directories.allowedValues.map(v => v.value);

            // Neue Konfiguration senden.
            return VCRServer.setDirectorySettings(settings);
        }

        // Ergänzt ein Verzeichnis.
        private onAdd(): JMSLib.App.IHttpPromise<void> {
            // Es erfolgt eine direkte Auswahl über eine Verzeichnisliste.
            if (this.showBrowse) {
                // Sicherheitshalber prüfen wir auf eine echte Auswahl.
                var selected = this.browse.value;
                if (selected)
                    this.addDirectory(selected);

                // Das geht synchron.
                return null;
            }

            // Der Anwender hat ein Netzwerkverzeichnis ausgewählt.
            var share = this.share.value.trim();

            // Prüfergebnis zurücksetzen.
            this._shareValidation = undefined;
            this.share.validate();

            // Oberfläche zur eingeschränkten Aktualisierung der Anzeige auffordern.
            if (this.share.site)
                this.share.site.refreshUi();

            // Verzeichnis durch den VCR.NET Recording Service prüfen lassen.
            return VCRServer.validateDirectory(share).then(ok => {
                // Gültige Verzeichnisse werden direkt in die Liste übernommen.
                if (ok) {
                    // Das brauchen wir jetzt nicht mehr.
                    this.share.value = null;

                    // Verzeichnisliste ergänzen.
                    this.addDirectory(share);
                }
                else {
                    // Ungültige Verzeichnisse setzen die Fehlerbedingung.
                    this._shareValidation = "Ungültiges Verzeichnis";
                    this.share.validate();

                    // Oberfläche zur eingeschränkten Aktualisierung der Anzeige auffordern.
                    if (this.share.site)
                        this.share.site.refreshUi();
                }
            });
        }

        // Übernimmt ein Verzeichnis in die Liste der Verzeichnisse.
        private addDirectory(folder: string): void {
            // Als Verzeichnis aufbereiten.
            folder = folder.trim();

            if (folder.length > 0)
                if (folder[folder.length - 1] != `\\`)
                    folder += `\\`;

            // Nur bisher unbekannte Verzeichnisse eintragen.
            if (!this.directories.allowedValues.some(v => v.value === folder))
                this.directories.allowedValues = this.directories.allowedValues.concat([JMSLib.App.uiValue(folder)]);
        }
    }
}