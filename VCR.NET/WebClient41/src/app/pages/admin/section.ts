namespace VCRNETClient.App.Admin {

    // Gemeinsame Schnittstelle für alle Konfigurationsbereiche der Administration.
    export interface ISection extends JMSLib.App.IConnectable {
        // Die zugehörige Konfigurationsseite.
        readonly page: IAdminPage;

        // Der Befehl zum Speichern der Daten des Konfigurationsbereichs.
        readonly update: JMSLib.App.ICommand;
    }

    // Basisklasse für einen Konfigurationsbereich.
    export abstract class Section<TSettingsType> implements ISection {

        // Der Befehl zum Speichern der Daten des Konfigurationsbereichs.
        private _update: JMSLib.App.Command<void>;

        // Meldet den Befehl zum Speichern, eventuell nach dem dieser einmalig erzeugt wurde.
        get update(): JMSLib.App.Command<void> {
            // Beim ersten Aufruf muss der Befehl erzeugt werden.
            if (!this._update)
                this._update = new JMSLib.App.Command(() => this.save(), this.saveCaption, () => this.isValid)

            return this._update;
        }

        // Erstellt einen neuen Konfigurationsbereich.
        constructor(public readonly page: AdminPage) {
        }

        // Das aktuell zur Anzeige veränderte Oberflächenelement.
        site: JMSLib.App.ISite;

        // Benachrichtigt das aktuelle Oberflächenelement über Veränderungen in der Anzeige.
        protected refreshUi(): void {
            if (this.site)
                this.site.refreshUi();
        }

        // Beginnt mit der eventuell erneuten Darstellung des Konfigurationsbreichs in der Oberfläche.
        reset(): void {
            // Schaltfläche aktualisieren.
            this.update.message = ``;

            // Aktuelle Daten vom VCR.NET Recording Service anfordern und Konfigurationsbereich damit initialisieren.
            this.loadAsync().then(settings => this.initialize(settings));
        }

        // Initialisiert den Bereich mit den aktuellen Konfigurationsdaten.
        protected abstract initialize(settings: TSettingsType);

        // Fordert die aktuelle Konfiguration an.
        protected abstract loadAsync(): JMSLib.App.IHttpPromise<TSettingsType>;

        // Meldet die Beschriftung des Befehls zum Speichern der Daten des Konfigurationsbereichs.
        protected readonly saveCaption: string = "Ändern";

        // Meldet ob die Daten des Konfigurationsbereichs gültig sind - nur dann ist der Befehl zum Speichern aktiv.
        protected readonly isValid: boolean = true;

        // Sendet die aktuellen Daten des Konfigurationsbereichs an den VCR.NET Recording Service zur Übernahme derselben.
        protected abstract saveAsync(): JMSLib.App.IHttpPromise<boolean>;

        // Beginnt mit der Speicherung der aktuellen Daten des Konfigurationsbereichs im VCR.NET Recording Service.
        private save(): JMSLib.App.IHttpPromise<void> {
            return this.page.update(this.saveAsync(), this.update);
        }
    }

    // Beschreibt einen Konfigurationsbereich.
    export interface ISectionInfo<TPageType extends ISection> {
        // Der eindeutige Name des Konfigurationsbereichs.
        readonly route: string;

        // Das Präsentationsmodell des Konfigurationsbereichs.
        readonly page: TPageType;
    }

}