/// <reference path="page.ts" />

namespace VCRNETClient.App.NoUi {

    // Die Auswahl eines Datumsfilters.
    export interface IPlanStartFilter extends IDisplayText {
        // Gesetzt, wenn der Filter aktiv ist.
        readonly active: boolean;

        // Aktiviert den Filter.
        activate(): void;
    }

    // Die Implementierung der Auswahl eines Datumsfilters.
    class PlanStartFilter implements IPlanStartFilter {
        // Gesetzt, wenn der Filter aktiv ist.
        active: boolean;

        // Der Anzeigetext des Filters.
        readonly text: string;

        // Erstellt einen neuen Filter.
        constructor(first: boolean, public readonly date: Date, private _activate: (filter: PlanStartFilter) => void) {
            // Sonderbehandlung für die Anzeige der ersten Auswahl. die zugleich auf vorgewählt ist.
            this.active = first;
            this.text = first ? "Jetzt" : DateFormatter.getShortDate(date);
        }

        // Aktiviert diesen Filter - und deaktiviert alle anderen.
        activate(): void {
            this._activate(this);
        }
    }

    export class PlanPage extends Page<INoUiSite> {
        private static _key = 0;

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

        private _startFilter: PlanStartFilter[];

        constructor(application: Application) {
            super("plan", application);

            this.navigation.plan = false;
            this.navigation.refresh = true;
        }

        reload(): void {
            this.query();
        }

        private activateFilter(filter: PlanStartFilter): void {
            this._startFilter.forEach(f => f.active = (f === filter));

            this.fireRefresh();
        }

        reset(section: string): void {
            var now = new Date();

            now = new Date(now.getFullYear(), now.getMonth(), now.getDate());

            this._startFilter = []

            var activate = this.activateFilter.bind(this);

            for (var i = 0; i < 8; i++) {
                this._startFilter.push(new PlanStartFilter(i === 0, now, activate));

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

            this.refreshUi();
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