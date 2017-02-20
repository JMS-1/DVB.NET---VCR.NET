/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IEditPage extends IPage {
        readonly job: Edit.IJobEditor;

        readonly schedule: Edit.IScheduleEditor;

        readonly save: JMSLib.App.ICommand;

        readonly del: JMSLib.App.ICommand;
    }

    export class EditPage extends Page implements IEditPage {
        private _jobScheduleInfo: VCRServer.JobScheduleInfoContract;

        job: Edit.JobEditor;

        schedule: Edit.ScheduleEditor;

        private _onChanged = this.onChanged.bind(this);

        save = new JMSLib.App.Command(() => this.onSave(), "Übernehmen", () => this._isValid);

        del = new JMSLib.App.Command(() => this.onDelete(), "Löschen", null);

        private _sources: VCRServer.SourceEntry[];

        private _isValid = false;

        private _fromGuide: boolean;

        constructor(application: Application) {
            super("edit", application);

            this.navigation.new = false;
        }

        // Wird jedesmal beim Aufruf der Änderungsseite aufgerufen.
        reset(sections: string[]): void {
            this.del.reset();
            this.save.reset();
            this.job = undefined;
            this._fromGuide = false;
            this.schedule = undefined;
            this.del.isDangerous = false;
            this._jobScheduleInfo = undefined;

            // Zuerst die Liste der Aufzeichnungsverzeichnisse abfragen.
            VCRServer.RecordingDirectoryCache.getPromise().then(dirs => this.setDirectories(dirs, sections));
        }

        // Die Liste der Aufzeichnungsverzeichnisse steht bereit.
        private setDirectories(folders: string[], sections: string[]): void {
            // Auswahlliste für den Anwender aufbauen.
            var folderSelection = folders.map(f => JMSLib.App.uiValue(f));

            folderSelection.unshift(JMSLib.App.uiValue("", "(Voreinstellung verwenden)"));

            // Geräteprofile anfordern.
            VCRServer.ProfileCache.getAllProfiles().then(profiles => this.setProfiles(profiles, sections, folderSelection));
        }

        // Die Liste der Geräteprofile steht bereit.
        private setProfiles(profiles: VCRServer.ProfileInfoContract[], sections: string[], folders: JMSLib.App.IUiValue<string>[]): void {
            // Auswahl für den Anwender vorbereiten.
            var profileSelection = profiles.map(p => JMSLib.App.uiValue(p.name));

            // Auf das Neuanlegen prüfen.
            if (sections.length < 1) {
                // Löschen geht sicher nicht.
                this.del.isVisible = false;

                // Leere Aufzeichnung angelegen.
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
                    schedule: this.createEmptySchedule()
                }

                this.setJobSchedule(info, profileSelection, folders);
            }
            else {
                // Auf existierende Aufzeichnung prüfen.
                var id = sections[0].substr(3);

                // Bei neuen Aufzeichnungen brauchen wir auch kein Löschen.
                this.del.isVisible = (id !== "*");

                // Einsprung aus der Programmzeitschrift.
                this._fromGuide = ((sections[1] || ``).substr(0, 6) === `epgid=`);

                // Existierende Aufzeichnung abrufen.
                VCRServer.createScheduleFromGuide(id, (sections[1] || "epgid=").substr(6)).then(info => this.setJobSchedule(info, profileSelection, folders));
            }
        }

        // Erstellt eine neue Aufzeichnung.
        private createEmptySchedule(): VCRServer.EditScheduleContract {
            var now = new Date(Date.now());

            return <VCRServer.EditScheduleContract>{
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
        }

        // Die Daten einer existierenden Aufzeichnung stehen bereit.
        private setJobSchedule(info: VCRServer.JobScheduleInfoContract, profiles: JMSLib.App.IUiValue<string>[], folders: JMSLib.App.IUiValue<string>[]): void {
            // Leere Aufzeichnung eintragen.
            if (!info.schedule)
                info.schedule = this.createEmptySchedule();

            // Liste der zuletzt verwendeten Quellen abrufen.
            var favorites = this.application.profile.recentSources || [];

            // Pflegemodelle anlegen.
            this._jobScheduleInfo = info;
            this.job = new Edit.JobEditor(this, info.job, profiles, favorites, folders, this._onChanged);
            this.schedule = new Edit.ScheduleEditor(this, info.schedule, favorites, this._onChanged);

            // Quellen für das aktuelle Geräteprofil laden und die Seite für den Anwender freigeben.
            this.loadSources().then(() => this.application.isBusy = false);
        }

        private loadSources(): JMSLib.App.IHttpPromise<VCRServer.SourceEntry[]> {
            var profile = this.job.device.value;

            return VCRServer.ProfileSourcesCache.getSources(profile).then(sources => {
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

        private onSave(): JMSLib.App.IHttpPromise<void> {
            var schedule = { ...this._jobScheduleInfo.schedule };

            var start = new Date(schedule.firstStart);
            var end = new Date(start.getFullYear(), start.getMonth(), start.getDate(), start.getHours(), start.getMinutes() + schedule.duration);

            schedule.duration = Math.floor((end.getTime() - start.getTime()) / 60000);

            return VCRServer
                .updateSchedule(this._jobScheduleInfo.jobId, this._jobScheduleInfo.scheduleId, { job: this._jobScheduleInfo.job, schedule: schedule })
                .then(() => {
                    if (this._fromGuide && this.application.profile.backToGuide)
                        this.application.gotoPage(this.application.guidePage.route);
                    else
                        this.application.gotoPage(this.application.planPage.route);
                });
        }

        private onDelete(): (JMSLib.App.IHttpPromise<void> | void) {
            if (this.del.isDangerous)
                return VCRServer
                    .deleteSchedule(this._jobScheduleInfo.jobId, this._jobScheduleInfo.scheduleId)
                    .then(() => this.application.gotoPage(this.application.planPage.route));

            this.del.isDangerous = true;
        }
    }
}