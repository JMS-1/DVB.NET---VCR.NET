namespace JMSLib.App {

    // Diese Schnittstelle wird von Views angeboten, die über notwendige Aktualisierungen der Oberfläche informiert werden möchten.
    export interface ISite {
        // Wird ausgelöst, sobald eine Aktualisierung notwendig ist.
        refreshUi(): void;
    }

    // Schnittstelle für NoUi Komponenten die über Änderungen benachrichtigen.
    export interface IConnectable {
        setSite(site: ISite): void;
    }
}