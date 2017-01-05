/// <reference path="noUi/page.ts" />

namespace VCRNETClient.App {
    export class EditPage extends NoUi.Page<NoUi.INoUiSite> {
        private _raw: VCRServer.JobScheduleInfoContract;

        private _job: NoUi.JobEditor;

        getJob(): NoUi.IJobEditor {
            return this._job;
        }

        private _schedule: NoUi.ScheduleEditor;

        getSchedule(): NoUi.IScheduleEditor {
            return this._schedule;
        }

        getSaveCommand(): NoUi.ICommand {
            return this._save;
        }

        private _loadFinished = this.loadFinished.bind(this);

        private _onChanged = this.onChanged.bind(this);

        private _refreshSite = this.refreshSite.bind(this);

        private _save = new NoUi.Command(() => this.onSave(), () => this._isValid, this._refreshSite, "Übernehmen");

        private _loadPending: number;

        private _sources: VCRServer.SourceEntry[];

        private _isValid = false;

        getRoute(): string {
            return "edit";
        }

        constructor(application: Application) {
            super(application);

            this.navigation.new = false;
        }

        reset(section: string): void {
            this._loadPending = 1;
            this._raw = undefined;
            this._job = undefined;
            this._schedule = undefined;

            VCRServer.RecordingDirectoryCache.load().then(dirs => {
                var folderSelection = dirs.map(f => <NoUi.ISelectableValue<string>>{ value: f, display: f });

                folderSelection.unshift(<NoUi.ISelectableValue<string>>{ value: "", display: "(Voreinstellung verwenden)" });

                return VCRServer.ProfileCache.load().then(profiles => {
                    var profileSelection = profiles.map(p => <NoUi.ISelectableValue<string>>{ value: p.name, display: p.name });

                    return VCRServer.createScheduleFromGuide(section.substr(3), "").then(info => {
                        var favorites = this.application.profile.recentSources || [];

                        this._raw = info;
                        this._job = new NoUi.JobEditor(info.job, profileSelection, favorites, folderSelection, this._onChanged);
                        this._schedule = new NoUi.ScheduleEditor(info.schedule, favorites, this._onChanged);

                        // Quellen für das aktuelle Geräteprofil laden.
                        return this.loadSources();
                    }).then(this._loadFinished);
                });
            });
        }

        private loadFinished(): void {
            if (--this._loadPending !== 0)
                return;

            this.application.setBusy(false);
        }

        private loadSources(): Thenable<VCRServer.SourceEntry[]> {
            var profile = this._job.device.val();

            return VCRServer.ProfileSourcesCache.load(profile).then(sources => {
                if (this._job.device.val() === profile) {
                    this._sources = sources;

                    this._job.validate(sources);
                    this._schedule.validate(sources, (this._job.source.val() || "").trim().length < 1);

                    this._isValid = this._job.isValid() && this._schedule.isValid();
                }

                return sources;
            });
        }

        getTitle(): string {
            return `Aufzeichnung bearbeiten`;
        }

        private refreshSite(): void {
            this.refresh();
        }

        private onChanged(): void {
            this.loadSources().then(this._refreshSite);

            this.refreshSite();
        }

        private onSave(): Thenable<void> {
            return VCRServer
                .updateSchedule(this._raw.jobId, this._raw.scheduleId, { job: this._raw.job, schedule: this._raw.schedule })
                .then(() => this.application.gotoPage(this.application.planPage.getRoute()));
        }
    }
}