/// <reference path="page.ts" />

namespace VCRNETClient.App {
    export class HomePage extends Page {
        getName(): string {
            return "home";
        }

        reset(): void {
            setTimeout(() => this.application.setBusy(false), 0);
        }

        constructor(application: Application) {
            super(application);
        }

        showNavigation(): boolean {
            return false;
        }
    }
}