/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IEditPage extends IPage {
        readonly job: IJobEditor;

        readonly schedule: IScheduleEditor;

        readonly save: JMSLib.App.ICommand;

        readonly del: JMSLib.App.ICommand;
    }

    export class EditPage extends Page<JMSLib.App.ISite> implements IEditPage {
        private _jobScheduleInfo: VCRServer.JobScheduleInfoContract;

        job: JobEditor;

        schedule: ScheduleEditor;

        private _onChanged = this.onChanged.bind(this);

        save = new JMSLib.App.Command(() => this.onSave(), "Übernehmen", () => this._isValid);

        del = new JMSLib.App.Command(() => this.onDelete(), "Löschen", null, false);

        private _sources: VCRServer.SourceEntry[];

        private _isValid = false;

        constructor(application: Application) {
            super("edit", application);

            this.navigation.new = false;
        }

        // Wird jedesmal beim Aufruf der Änderungsseite aufgerufen.
        reset(sections: string[]): void {
            this.job = undefined;
            this.schedule = undefined;
            this.del.isDangerous = false;
            this._jobScheduleInfo = undefined;

            // Zuerst die Liste der Aufzeichnungsverzeichnisse abfragen.
            VCRServer.RecordingDirectoryCache.getPromise().then(dirs => this.setDirectories(dirs, sections));
        }

        // Die Liste der Aufzeichnungsverzeichnisse steht bereit.
        private setDirectories(folders: string[], sections: string[]): void {
            // Auswahlliste für den Anwender aufbauen.
            var folderSelection = folders.map(f => <JMSLib.App.IUiValue<string>>{ value: f, display: f });

            folderSelection.unshift(<JMSLib.App.IUiValue<string>>{ value: "", display: "(Voreinstellung verwenden)" });

            // Geräteprofile anfordern.
            VCRServer.ProfileCache.getPromise().then(profiles => this.setProfiles(profiles, sections, folderSelection));
        }

        // Die Liste der Geräteprofile steht bereit.
        private setProfiles(profiles: VCRServer.ProfileInfoContract[], sections: string[], folders: JMSLib.App.IUiValue<string>[]): void {
            // Auswahl für den Anwender vorbereiten.
            var profileSelection = profiles.map(p => <JMSLib.App.IUiValue<string>>{ value: p.name, display: p.name });

            // Auf das Neuanlegen prüfen.
            if (sections.length < 1) {
                var now = new Date(Date.now());

                var newSchedule = <VCRServer.EditScheduleContract>{
                    firstStart: new Date(now.getFullYear(), now.getMonth(), now.getDate(), now.getHours(), now.getMinutes()).toISOString(),
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
                // Auf existierende Aufzeichnung prüfen.
                var id = sections[0].substr(3);

                this.del.isVisible = (id !== "*");

                // Existierende Aufzeichnung abrufen.
                VCRServer.createScheduleFromGuide(id, (sections[1] || "epgid=").substr(6)).then(info => this.setJobSchedule(info, profileSelection, folders));
            }
        }

        // Die Daten einer existierenden Aufzeichnung stehen bereit.
        private setJobSchedule(info: VCRServer.JobScheduleInfoContract, profiles: JMSLib.App.IUiValue<string>[], folders: JMSLib.App.IUiValue<string>[]): void {
            // Liste der zuletzt verwendeten Quellen abrufen.
            var favorites = this.application.profile.recentSources || [];

            // Pflegemodelle anlegen.
            this._jobScheduleInfo = info;
            this.job = new JobEditor(this, info.job, profiles, favorites, folders, this._onChanged);
            this.schedule = new ScheduleEditor(this, info.schedule, favorites, this._onChanged);

            // Quellen für das aktuelle Geräteprofil laden und die Seite für den Anwender freigeben.
            this.loadSources().then(() => this.application.setBusy(false));
        }

        private loadSources(): JMSLib.App.Thenable<VCRServer.SourceEntry[], XMLHttpRequest> {
            var profile = this.job.device.value;

            return VCRServer.ProfileSourcesCache.getPromise(profile).then(sources => {
                if (this.job.device.value === profile) {
                    this._sources = sources;

                    this.job.validate(sources);
                    this.schedule.validate(sources, (this.job.source.value || "").trim().length < 1);

                    this._isValid = this.job.isValid() && this.schedule.isValid();
                }

                return sources;
            });
        }

        get title(): string {
            return this.del.isVisible ? `Aufzeichnung bearbeiten` : `Neue Aufzeichnung anlegen`;
        }

        private onChanged(): void {
            this.loadSources().then(() => this.refreshUi());

            this.refreshUi();
        }

        private onSave(): JMSLib.App.Thenable<void, XMLHttpRequest> {
            return VCRServer
                .updateSchedule(this._jobScheduleInfo.jobId, this._jobScheduleInfo.scheduleId, { job: this._jobScheduleInfo.job, schedule: this._jobScheduleInfo.schedule })
                .then(() => this.application.gotoPage(this.application.planPage.route));
        }

        private onDelete(): (JMSLib.App.Thenable<void, XMLHttpRequest> | void) {
            if (this.del.isDangerous)
                return VCRServer
                    .deleteSchedule(this._jobScheduleInfo.jobId, this._jobScheduleInfo.scheduleId)
                    .then(() => this.application.gotoPage(this.application.planPage.route));

            this.del.isDangerous = true;
        }
    }
}