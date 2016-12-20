/// <reference path="page.ts" />

namespace VCRNETClient.App {
    export interface IPlanSite {
        onRefresh(jobs: PlanEntry[], index: number, showTasks: boolean): void;
    }

    export interface IPlanStartFilter {
        index: number;

        date: Date;
    }

    export class PlanPage extends Page {
        private static _key = 0;

        getName(): string {
            return "plan";
        }

        private _site: IPlanSite;

        private _jobs: PlanEntry[];

        private _filter: (job: PlanEntry) => boolean = this.filterJob.bind(this);

        private _startIndex = 0;

        private _showTasks = false;

        private _startFilter: IPlanStartFilter[];

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
            var now = new Date();

            now = new Date(now.getFullYear(), now.getMonth(), now.getDate());

            this._startFilter = []

            for (var i = 0; i < 8; i++) {
                this._startFilter.push({ index: i, date: now });

                now = new Date(now.getFullYear(), now.getMonth(), now.getDate() + this.application.profile.planDays);
            }

            this._showTasks = false;
            this._site = undefined;
            this._jobs = undefined;
            this._startIndex = 0;

            this.reload();
        }

        setSite(site: IPlanSite): void {
            if (this._site === site)
                return;

            this._site = site;

            this.fireRefresh();
        }

        private filterJob(job: PlanEntry): boolean {
            var startDay = this._startFilter[this._startIndex].date;
            var endDay = this._startFilter[this._startIndex + 1].date;

            if (job.start >= endDay)
                return false;
            else if (job.end <= startDay)
                return false;

            if (!this._showTasks)
                if (!job.mode)
                    return false;

            return true;
        }

        private reload(): void {
            // Wir schauen maximal 13 Wochen in die Zukunft
            var endOfTime = new Date(Date.now() + 13 * 7 * 86400000);

            // Zusätzlich beschränken wir uns auf maximal 500 Einträge
            VCRServer.getPlan(500, endOfTime).done((raw: any[]) => {
                this._jobs = raw.map(job => new PlanEntry(job, `${PlanPage._key++}`));

                this.fireRefresh();

                this.application.setBusy(false);
            });
        }

        private fireRefresh(): void {
            if (this._site && this._jobs)
                this._site.onRefresh(this._jobs.filter(this._filter), this._startIndex, this._showTasks);
        }

        filterOnStart(index: number): void {
            if (index === this._startIndex)
                return;

            this._startIndex = index;
            this.fireRefresh();
        }

        toggleTaskFilter(): void {
            this._showTasks = !this._showTasks;
            this.fireRefresh();
        }

        getStartFilter(): IPlanStartFilter[] {
            return this._startFilter.slice(0, this._startFilter.length - 1);
        }

        getTitle(): string {
            var days = this.application.profile.planDays;

            return `Geplante Aufzeichnungen für ${days} Tag${(days === 1) ? "" : "e"}`;
        }
    }
}