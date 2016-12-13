/// <reference path="page.ts" />

namespace VCRNETClient.App {
    export class HomePage extends Page {
        static readonly name = "home";

        getName(): string {
            return HomePage.name;
        }

        constructor(application: Application) {
            super(application);
        }
    }
}