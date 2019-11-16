namespace JMSLib.App {

    // Beschreibt einen Fehler bei einem HTTP Aufruf.
    export interface IHttpErrorInformation {
        // Die Fehlermeldung.
        readonly message: string;

        // Details zu Fehlermeldung.
        readonly details: string;
    }

}