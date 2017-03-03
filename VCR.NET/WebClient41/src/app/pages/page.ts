namespace VCRNETClient.App {

    // Die äußere Sicht auf eine Seite der Anwendung.
    export interface IPage extends JMSLib.App.IConnectable {
        // Rückgriff auf die Anwendung als Ganzes.
        readonly application: IApplication;

        // Der eindeutige Name (der Navigationsbereich) zur Seite.
        readonly route: string;

        // Die Überschrift der Seite.
        readonly title: string;

        // Konfiguration der Navigationsleiste.
        readonly navigation: {
            // Aktualisierung.
            readonly refresh: boolean;

            // Aufzeichnungsplan.
            readonly plan: boolean;

            // Programmzeitschrift.
            readonly guide: boolean;

            // Suchfavoriten.
            readonly favorites: boolean;

            // Neu anlegen.
            readonly new: boolean;

            // Laufende Aufzeichnungen.
            readonly current: boolean;
        };

        // Fordert zur Aktualisierung der zur Seite gehörenden Daten auf.
        reload(): void;
    }

    // Basisklasse zur Implementierung von Seiten.
    export abstract class Page implements IPage {

        // Das zugehörige Oberflächenelement.
        view: JMSLib.App.IView;

        // Meldet Änderungen an das zugehörige Oberflächenelement.
        protected refreshUi(): void {
            if (this.view)
                this.view.refreshUi();
        }

        // Initialisiert die Seite zur erneuten Anzeige.
        abstract reset(sections: string[]): void;

        // Meldet die Überschrift der Seite.
        abstract get title(): string;

        // Initialisiert die Navigationsleiste.
        navigation = {
            favorites: false,
            refresh: false,
            current: true,
            guide: true,
            plan: true,
            new: true
        };

        // Initialisiert eine neue Seite.
        constructor(public readonly route: string, public readonly application: Application) {
        }

        // Aktualisiert den Inhalt der Seite.
        reload(): void {
        }
    }

}