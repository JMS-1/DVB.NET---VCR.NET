/// <reference path="page.ts" />

namespace VCRNETClient.App {
    export interface IPlanSite {
        onRefresh(jobs: PlanEntry[]): void;
    }

    export class PlanPage extends Page {
        private static _key = 0;

        static readonly name = "plan";

        getName(): string {
            return PlanPage.name;
        }

        private _site: IPlanSite;

        private _jobs: PlanEntry[];

        private _filter: (job: PlanEntry) => boolean = this.filterJob.bind(this);

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
            this.reload();
        }

        reset(): void {
            this._site = undefined;
            this._jobs = undefined;

            this.reload();
        }

        setSite(site: IPlanSite): void {
            if (this._site === site)
                return;

            this._site = site;

            this.fireRefresh();
        }

        private filterJob(job: PlanEntry): boolean {
            return true;
        }

        private reload(): void {
            // Wir schauen maximal 13 Wochen in die Zukunft
            var endOfTime = new Date(Date.now() + 13 * 7 * 86400000);

            // Zusätzlich beschränken wir uns auf maximal 500 Einträge
            VCRServer.getPlan(500, endOfTime).done((raw: any[]) => {
                this._jobs = raw.map(job => new PlanEntry(job, PlanPage._key++));

                this.fireRefresh();

                this.application.setBusy(false);
            });
        }

        private fireRefresh(): void {
            if (this._site && this._jobs)
                this._site.onRefresh(this._jobs.filter(this._filter));
        }
    }
}