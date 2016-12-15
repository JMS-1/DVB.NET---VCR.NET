/// <reference path="page.ts" />

namespace VCRNETClient.App {
    export class PlanPage extends Page {
        static readonly name = "plan";

        getName(): string {
            return PlanPage.name;
        }

        reset(): void {
            setTimeout(() => this.application.setBusy(false), 0);
        }

        constructor(application: Application) {
            super(application);
        }

        showPlan(): boolean {
            return false;
        }

        showRefresh(): boolean {
            return true;
        }

        onRefresh(): void {
        }
    }
}