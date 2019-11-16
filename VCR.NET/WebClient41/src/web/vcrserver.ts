/// <reference path="../lib/http/config.ts" />

module VCRServer {
    // Normalerweise sind wir das selbst
    var serverRoot = document.URL.substr(0, document.URL.indexOf(`/`, document.URL.indexOf(`://`) + 3));

    // Schauen wir uns mal die Betriebsart an
    var query = window.location.search;
    if (query == `?debug`)
        serverRoot = `http://localhost:81`;

    // Der Präfix für den Zugriff auf Geräte und Dateien
    var protocolEnd = serverRoot.indexOf(`://`);
    var deviceUrl = `dvbnet` + serverRoot.substr(protocolEnd) + `/`;

    // Der Präfix für alle REST Zugiffe
    JMSLib.App.webCallRoot = serverRoot + `/vcr.net/`;

    // Führt eine Web Anfrage aus.
    export function doUrlCall<TResponseType, TRequestType>(url: string, method: string = `GET`, request?: TRequestType): Promise<TResponseType> {
        return JMSLib.App.browserWebCall(url, method, request);
    }

    // Meldet den Verweis zum Aufruf des DVB.NET / VCR.NET Viewers.
    export function getDeviceRoot(): string {
        return deviceUrl;
    }

    // Meldet den Pfad zum Abspielen einer abgeschlossenen Aufzeichnung (DVB.NET / VCR.NET Viewer muss installiert sein).
    export function getFilePlayUrl(path: string): string {
        return `${getDeviceRoot()}play=${encodeURIComponent(path)}`;
    }

}

