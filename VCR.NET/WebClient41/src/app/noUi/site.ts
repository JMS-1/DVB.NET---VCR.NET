﻿namespace VCRNETClient.App.NoUi {

    // Diese Schnittstelle wird von Views angeboten, die über notwendige Aktualisierungen der Oberfläche informiert werden möchten.
    export interface IPageSite {
        // Wird ausgelöst, sobald eine Aktualisierung notwendig ist.
        refreshUi(): void;
    }

    // Schnittstelle für NoUi Komponenten die über Änderungen benachrichtigen.
    export interface INoUiWithSite {
        setSite(site: IPageSite): void;
    }
}