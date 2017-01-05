/// <reference path="page.ts" />

namespace VCRNETClient.App.NoUi {
    export interface IPlanStartFilter {
        active: boolean;

        activate: () => void;

        date: Date;
    }

    export class PlanPage extends Page<INoUiSite> {
        private static _key = 0;

        getRoute(): string {
            return "plan";
        }

        private _jobs: IPlanEntry[];

        getJobs(): IPlanEntry[] {
            if (this._jobs)
                for (var i = 0; i < this._startFilter.length - 1; i++)
                    if (this._startFilter[i].active)
                        return this._jobs.filter(job => this.filterJob(job, i));

            return null;
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

        onReload(): void {
            this.query();
        }

        reset(section: string): void {
            var now = new Date();

            now = new Date(now.getFullYear(), now.getMonth(), now.getDate());

            this._startFilter = []

            for (var i = 0; i < 8; i++) {
                let filter: IPlanStartFilter;

                filter = {
                    active: i === 0,
                    date: now,
                    activate: () => {
                        this._startFilter.forEach(f => f.active = false);

                        filter.active = true;

                        this.fireRefresh();
                    }
                };

                this._startFilter.push(filter);

                now = new Date(now.getFullYear(), now.getMonth(), now.getDate() + this.application.profile.planDays);
            }

            this._showTasks = false;
            this._jobs = undefined;

            this.query();
        }

        protected onSiteChanged(): void {
            this.fireRefresh();
        }

        private filterJob(job: IPlanEntry, startIndex: number): boolean {
            var startDay = this._startFilter[startIndex].date;
            var endDay = this._startFilter[startIndex + 1].date;

            if (job.start >= endDay)
                return false;
            else if (job.end <= startDay)
                return false;

            if (!this._showTasks)
                if (!job.mode)
                    return false;

            return true;
        }

        private query(): void {
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

            this.refresh();
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