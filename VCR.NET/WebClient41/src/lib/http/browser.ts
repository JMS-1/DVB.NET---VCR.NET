/// <reference path='config.ts' />

namespace JMSLib.App {

    // Verwendet den XHMLHttpRequest des Browser zur Durchführung eines HTTP Aufrufs.
    export function browserWebCall<TResponseType, TRequestType>(url: string, method: string = 'GET', request?: TRequestType): IHttpPromise<TResponseType> {
        // Eindeutige Nummer für den nächsten HTTP Aufruf ermitteln - tatsächlich arbeiten wir hier in 2er Schritten, aber das tut nicht zur Sache.
        var nextId = nextWebCallId() + 1;

        // Aynchronen Aufruf aufsetzen.
        return new Promise<TResponseType, IHttpErrorInformation>((success, failure) => {
            // Aufruf an eine absolute URL erkennen.
            var raw = (url.substr(0, 7) === "http://");

            // HTTP Aufruf anlegen.
            var xhr = new XMLHttpRequest();

            // HTTP Antwort abwarten.
            xhr.addEventListener("load", () => {
                // Sicherstellen, dass die Antwort überhaupt noch interessiert.
                if (nextWebCallId() != nextId)
                    return;

                // Ergebnis auswerten.
                if (xhr.status < 400)
                    // Antwort wandeln und melden.
                    if (xhr.status === 204)
                        success(undefined);
                    else if (raw)
                        success(<any>xhr.responseText);
                    else
                        success(JSON.parse(xhr.responseText));
                else {
                    // Fehler auswerten.
                    var errorInfo = JSON.parse(xhr.responseText);

                    // Fehler melden - falls es jemanden interessiert.
                    failure(<IHttpErrorInformation>{ message: errorInfo.ExceptionMessage || errorInfo.Message, details: errorInfo.MessageDetails });
                }
            });

            // Im Falle einer relativen URL den Bezugspunkt nach Angabe des Clients anpassen.
            if (!raw)
                url = webCallRoot + url;

            // Endpunkt einrichten.
            xhr.open(method, url);

            // Antwortformat anmelden.
            if (!raw)
                xhr.setRequestHeader("accept", "application/json");

            // Aufruf absetzen.
            if (request === undefined) {
                // Aufruf ohne Eingangsparameter.
                xhr.send();
            }
            else {
                // Als Eingangsparameter kennen wir nur JSON.
                xhr.setRequestHeader("content-type", "application/json");

                // Eingangsparameter wandeln und übertragen.
                xhr.send(JSON.stringify(request));
            }
        });
    }

}