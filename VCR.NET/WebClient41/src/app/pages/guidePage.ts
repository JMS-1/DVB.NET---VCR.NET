/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export class GuidePage extends Page<JMSLib.App.ISite> {
        constructor(application: Application) {
            super("guide", application);

            // Navigation abweichend vom Standard konfigurieren.
            this.navigation.favorites = true;
            this.navigation.guide = false;
        }

        reset(section: string): void {
            setTimeout(() => this.application.setBusy(false), 0);
        }

        get title(): string {
            return "Programmzeitschrift";
        }
    }
}