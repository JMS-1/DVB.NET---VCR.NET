/// <reference path="page.ts" />

namespace VCRNETClient.App {
    export class HomePage extends Page {
        static readonly name = "home";

        getName(): string {
            return HomePage.name;
        }

        reset(): void {
            setTimeout(() => this.application.setBusy(false), 0);
        }

        constructor(application: Application) {
            super(application);
        }
    }
}