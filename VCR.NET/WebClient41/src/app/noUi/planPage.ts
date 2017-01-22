/// <reference path="page.ts" />

namespace VCRNETClient.App.NoUi {

    // Die Auswahl eines Datumsfilters.
    export interface IPlanStartFilter extends JMSLib.App.IDisplayText {
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

    // Steuert die Anzeige des Aufzeichnungsplan.
    export class PlanPage extends Page<JMSLib.App.INoUiSite> {
        // Alle aktuell bekannten Aufträge
        private _jobs: PlanEntry[] = [];

        // Ermittelt die aktuell anzuzeigenden Aufräge.
        getJobs(): IPlanEntry[] {
            for (var i = 0; i < this._startFilter.length - 1; i++)
                if (this._startFilter[i].active)
                    return this._jobs.filter(job => this.filterJob(job, i));

            return null;
        }

        // Gesetzt, wenn auch alle Aufgaben angezeigt werden.
        private _showTasks = false;

        showTasks(): boolean {
            return this._showTasks;
        }

        // Alle bekannten Datumsfilter.
        private _startFilter: PlanStartFilter[];

        // Erzeugt eine neuie Steuerung.
        constructor(application: Application) {
            super("plan", application);

            // Navigation abweichend vom Standard konfigurieren.
            this.navigation.plan = false;
            this.navigation.refresh = true;
        }

        // Daten neu anfordern.
        reload(): void {
            this.query();
        }

        // Wählt einen bestimmten Datumsbereich aus.
        private activateFilter(filter: PlanStartFilter): void {
            this._startFilter.forEach(f => f.active = (f === filter));

            this.fireRefresh();
        }

        // Wird beim Aufruf der Seite aktiviert.
        reset(section: string): void {
            // Aktuelles Dateum ermitteln.
            var now = new Date();

            now = new Date(now.getFullYear(), now.getMonth(), now.getDate());

            // Datumsfilter basierend darauf erstellen.
            this._startFilter = []

            var activate = this.activateFilter.bind(this);

            for (var i = 0; i < 8; i++) {
                this._startFilter.push(new PlanStartFilter(i === 0, now, activate));

                now = new Date(now.getFullYear(), now.getMonth(), now.getDate() + this.application.profile.planDays);
            }

            // Internen Zustand zurück setzen
            this._showTasks = false;
            this._jobs = [];

            // Daten erstmalig anfordern.
            this.query();
        }

        // Prüft, ob ein Auftrag den aktuellen Einschränkungen entspricht.
        private filterJob(job: PlanEntry, startIndex: number): boolean {
            // Datumsfilter.
            var startDay = this._startFilter[startIndex].date;
            var endDay = this._startFilter[startIndex + 1].date;

            if (job.start >= endDay)
                return false;
            else if (job.end <= startDay)
                return false;

            // Aufgabenfilter.
            if (!this._showTasks)
                if (!job.mode)
                    return false;

            return true;
        }

        // Ermittelt die aktuell gültigen Aufträge.
        private query(): void {
            // Wir schauen maximal 13 Wochen in die Zukunft
            var endOfTime = new Date(Date.now() + 13 * 7 * 86400000);

            // Zusätzlich beschränken wir uns auf maximal 500 Einträge
            VCRServer.getPlan(500, endOfTime).then(plan => {
                var toggleDetail = this.toggleDetail.bind(this);
                var reload = this.reload.bind(this);

                this._jobs = plan.map(job => new PlanEntry(job, toggleDetail, this.application, reload));

                this.fireRefresh();

                this.application.setBusy(false);
            });
        }

        // Schaltet die Detailanzeige für einen Auftrag um.
        private toggleDetail(job: PlanEntry, epg: boolean): void {
            // Anzeige einfach nur ausblenden.
            if (job.showEpg && epg)
                job.showEpg = false;
            else if (job.showException && !epg)
                job.showException = false;
            else {
                // Gewünschte Anzeige für genau diesen einen Auftrag aktivieren.
                this._jobs.forEach(j => j.showEpg = j.showException = false);

                if (epg)
                    job.showEpg = true;
                else
                    job.showException = true;
            }

            // Anzeige aktualisieren.
            this.fireRefresh(false);
        }

        // Anzeige aktualisieren.
        private fireRefresh(full = true): void {
            if (full && this._jobs)
                this._jobs.forEach(j => j.showEpg = j.showException = false);

            this.refreshUi();
        }

        // Schaltet die Anzeige der Aufgaben um.
        toggleTaskFilter(): void {
            this._showTasks = !this._showTasks;

            this.fireRefresh();
        }

        // Ermittelt die Liste der Datumsfilter.
        getStartFilter(): IPlanStartFilter[] {
            // Wir hängen immer am Ende einen unsichtbaren an, der die Prüfung auf Datumsbereich deutlich vereinfacht.
            return this._startFilter.slice(0, this._startFilter.length - 1);
        }

        // Meldet die Überschrift der Seite.
        get title(): string {
            var days = this.application.profile.planDays;

            return `Geplante Aufzeichnungen für ${days} Tag${(days === 1) ? "" : "e"}`;
        }
    }
}