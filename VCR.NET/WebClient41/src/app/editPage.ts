/// <reference path="noUi/page.ts" />

namespace VCRNETClient.App {
    export class EditPage extends NoUi.Page<NoUi.INoUiSite> {
        private _jobScheduleInfo: VCRServer.JobScheduleInfoContract;

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

        private _onChanged = this.onChanged.bind(this);

        private _refreshSite = this.refreshSite.bind(this);

        private _save = new NoUi.Command(() => this.onSave(), () => this._isValid, this._refreshSite, "Übernehmen");

        private _sources: VCRServer.SourceEntry[];

        private _isValid = false;

        constructor(application: Application) {
            super("edit", application);

            this.navigation.new = false;
        }

        // Wird jedesmal beim Aufruf der Änderungsseite aufgerufen.
        reset(section: string): void {
            this._jobScheduleInfo = undefined;
            this._job = undefined;
            this._schedule = undefined;

            // Zuerst die Liste der Aufzeichnungsverzeichnisse abfragen.
            VCRServer.RecordingDirectoryCache.getPromise().then(dirs => this.setDirectories(dirs, section));
        }

        // Die Liste der Aufzeichnungsverzeichnisse steht bereit.
        private setDirectories(folders: string[], section: string): void {
            // Auswahlliste für den Anwender aufbauen.
            var folderSelection = folders.map(f => <NoUi.ISelectableValue<string>>{ value: f, display: f });

            folderSelection.unshift(<NoUi.ISelectableValue<string>>{ value: "", display: "(Voreinstellung verwenden)" });

            // Geräteprofile anfordern.
            VCRServer.ProfileCache.getPromise().then(profiles => this.setProfiles(profiles, section, folderSelection));
        }

        // Die Liste der Geräteprofile steht bereit.
        private setProfiles(profiles: VCRServer.ProfileInfoContract[], section: string, folders: NoUi.ISelectableValue<string>[]): void {
            // Auswahl für den Anwender vorbereiten.
            var profileSelection = profiles.map(p => <NoUi.ISelectableValue<string>>{ value: p.name, display: p.name });

            // Auf das Neuanlegen prüfen.
            if (section.length < 1) {
                var now = new Date(Date.now());

                var newSchedule = <VCRServer.EditScheduleContract>{
                    firstStart: new Date(now.getFullYear(), now.getMonth(), now.getDate(), now.getHours(), now.getMinutes()).toISOString(),
                    lastDay: NoUi.ScheduleEditor.makePureDate(NoUi.ScheduleEditor.maximumDate).toISOString(),
                    withSubtitles: this.application.profile.subtitles,
                    withVideotext: this.application.profile.videotext,
                    allLanguages: this.application.profile.languages,
                    includeDolby: this.application.profile.dolby,
                    repeatPattern: 0,
                    exceptions: null,
                    sourceName: '',
                    duration: 120,
                    name: ''
                };

                var newJob = <VCRServer.EditJobContract>{
                    withSubtitles: this.application.profile.subtitles,
                    withVideotext: this.application.profile.videotext,
                    allLanguages: this.application.profile.languages,
                    includeDolby: this.application.profile.dolby,
                    device: profiles[0] && profiles[0].name,
                    lockedToDevice: false,
                    sourceName: '',
                    directory: '',
                    name: ''
                };

                var info = <VCRServer.JobScheduleInfoContract>{
                    job: newJob,
                    jobId: null,
                    scheduleId: null,
                    schedule: newSchedule
                }

                this.setJobSchedule(info, profileSelection, folders);
            }
            else {
                // Existierende Aufzeichnung abrufen.
                VCRServer.createScheduleFromGuide(section.substr(3), "").then(info => this.setJobSchedule(info, profileSelection, folders));
            }
        }

        // Die Daten einer existierenden Aufzeichnung stehen bereit.
        private setJobSchedule(info: VCRServer.JobScheduleInfoContract, profiles: NoUi.ISelectableValue<string>[], folders: NoUi.ISelectableValue<string>[]): void {
            // Liste der zuletzt verwendeten Quellen abrufen.
            var favorites = this.application.profile.recentSources || [];

            // Pflegemodelle anlegen.
            this._jobScheduleInfo = info;
            this._job = new NoUi.JobEditor(this, info.job, profiles, favorites, folders, this._onChanged);
            this._schedule = new NoUi.ScheduleEditor(this, info.schedule, favorites, this._onChanged);

            // Quellen für das aktuelle Geräteprofil laden und die Seite für den Anwender freigeben.
            this.loadSources().then(() => this.application.setBusy(false));
        }

        private loadSources(): Thenable<VCRServer.SourceEntry[], XMLHttpRequest> {
            var profile = this._job.device.val();

            return VCRServer.ProfileSourcesCache.getPromise(profile).then(sources => {
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
            this.refreshUi();
        }

        private onChanged(): void {
            this.loadSources().then(this._refreshSite);

            this.refreshSite();
        }

        private onSave(): Thenable<void, XMLHttpRequest> {
            return VCRServer
                .updateSchedule(this._jobScheduleInfo.jobId, this._jobScheduleInfo.scheduleId, { job: this._jobScheduleInfo.job, schedule: this._jobScheduleInfo.schedule })
                .then(() => this.application.gotoPage(this.application.planPage.route));
        }
    }
}