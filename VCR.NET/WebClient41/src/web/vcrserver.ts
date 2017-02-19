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
    var playUrl = deviceUrl + `play=`;

    // Der Präfix für alle REST Zugiffe
    var restRoot = serverRoot + `/vcr.net/`;

    // Bibliothek konfigurieren.
    JMSLib.App.webCallRoot = restRoot;

    export function doUrlCall<TResponseType, TRequestType>(url: string, method: string = `GET`, request?: TRequestType): JMSLib.App.IHttpPromise<TResponseType> {
        return JMSLib.App.browserWebCall(url, method, request);
    }

    export function getRestRoot(): string {
        return restRoot;
    }

    export function getServerRoot(): string {
        return serverRoot;
    }

    export function getFilePlayUrl(path: string): string {
        return `${playUrl}${encodeURIComponent(path)}`;
    }

    export function getDeviceRoot(): string {
        return deviceUrl;
    }

}

