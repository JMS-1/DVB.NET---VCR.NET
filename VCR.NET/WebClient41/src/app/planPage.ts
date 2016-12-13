/// <reference path="page.ts" />

namespace VCRNETClient.App {
    export class PlanPage extends Page {
        static readonly name = "plan";

        getName(): string {
            return PlanPage.name;
        }

        constructor(application: Application) {
            super(application);
        }
    }
}