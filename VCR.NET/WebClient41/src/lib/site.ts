namespace JMSLib.App {

    // Diese Schnittstelle wird von Views angeboten, die über notwendige Aktualisierungen der Oberfläche informiert werden möchten.
    export interface ISite {
        // Wird ausgelöst, sobald eine Aktualisierung notwendig ist.
        refreshUi(): void;
    }

    // Schnittstelle für Ui View Modelle, die über Änderungen benachrichtigen können.
    export interface IConnectable {
        // Das eindeutige aktuell zur Anzeige verwendete Oberflächenelement.
        site: ISite;
    }
}