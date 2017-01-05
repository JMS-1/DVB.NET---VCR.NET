/// <reference path="page.ts" />

namespace VCRNETClient.App.NoUi {

    // Die Anwendungslogik für die Startseite.
    export class HomePage extends Page {

        // Meldet den eindeutigen Kurznamen (die Route) der Startseite.
        getRoute(): string {
            return "home";
        }

        // Zeigt die Startseite (erneut) an.
        reset(section: string): void {
            setTimeout(() => this.application.setBusy(false), 0);
        }

        // Meldet, dass die Navigationsleiste nicht angezeigt werden soll.
        showNavigation(): boolean {
            return false;
        }

        // Meldet die Überschrift der Startseite.
        getTitle(): string {
            // Der Titel wird dem aktuellen Kenntnisstand angepasst.
            var version = this.application.version;
            var title = this.application.getTitle();

            if (version)
                return `${title} (${version.msiVersion})`;
            else
                return title;
        }
    }
}