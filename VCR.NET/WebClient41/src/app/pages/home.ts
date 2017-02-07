﻿/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IHomePage extends IPage {
    }

    // Die Anwendungslogik für die Startseite.
    export class HomePage extends Page implements IHomePage {
        // Erstellt die Anwendungslogik.
        constructor(application: Application) {
            super("home", application);

            // Meldet, dass die Navigationsleiste nicht angezeigt werden soll.
            this.navigation = null;
        }

        // Zeigt die Startseite (erneut) an.
        reset(sections: string[]): void {
            setTimeout(() => this.application.isBusy = false, 0);
        }

        // Meldet die Überschrift der Startseite.
        get title(): string {
            // Der Titel wird dem aktuellen Kenntnisstand angepasst.
            var version = this.application.version;
            var title = this.application.title;

            if (version)
                return `${title} (${version.msiVersion})`;
            else
                return title;
        }
    }
}