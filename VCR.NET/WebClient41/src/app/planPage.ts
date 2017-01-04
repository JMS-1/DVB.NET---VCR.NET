/// <reference path="page.ts" />

namespace VCRNETClient.App {
    export interface IPlanSite {
        onRefresh(): void;
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

        private _jobs: IPlanEntry[];

        getJobs(): IPlanEntry[] {
            return this._jobs && this._jobs.filter(this._filter);
        }

        private _filter: (job: IPlanEntry) => boolean = this.filterJob.bind(this);

        private _startIndex = 0;

        getIndex(): number {
            return this._startIndex;
        }

        private _showTasks = false;

        showTasks(): boolean {
            return this._showTasks;
        }

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

        reset(section: string): void {
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
            VCRServer.getPlan(500, endOfTime).then(plan => {
                var toggleDetail = this.toggleDetail.bind(this);

                this._jobs = plan.map(job => enrichPlanEntry(job, `${PlanPage._key++}`, toggleDetail));

                this.fireRefresh();

                this.application.setBusy(false);
            });
        }

        private toggleDetail(job: IPlanEntry, epg: boolean): void {
            if (job.showEpg && epg)
                job.showEpg = false;
            else if (job.showException && !epg)
                job.showException = false;
            else {
                this._jobs.forEach(j => j.showEpg = j.showException = false);

                if (epg)
                    job.showEpg = true;
                else
                    job.showException = true;
            }

            this.fireRefresh(false);
        }

        private fireRefresh(full = true): void {
            if (full && this._jobs)
                this._jobs.forEach(j => j.showEpg = j.showException = false);

            if (this._site)
                this._site.onRefresh();
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