﻿/// <reference path="page.ts" />

namespace VCRNETClient.App {

    // Schnittstelle zur Pflege einer einzelnen Aufzeichnung.
    export interface IEditPage extends IPage {
        // Die Daten des zugehörigen Auftrags.
        readonly job: Edit.IJobEditor;

        // Die Daten der Aufzeichnung.
        readonly schedule: Edit.IScheduleEditor;

        // Befehl zum Speichern der Aufzeichnung.
        readonly save: JMSLib.App.ICommand;

        // Befehl zum Löschen der Aufzeichnung.
        readonly del: JMSLib.App.ICommand;
    }

    // Das Präsentationsmodell zur Pflege einer Aufzeichnung.
    export class EditPage extends Page implements IEditPage {

        // Die Originaldaten der Aufzeichnung.
        private _jobScheduleInfo: VCRServer.JobScheduleInfoContract;

        // Die Daten des zugehörigen Auftrags.
        job: Edit.JobEditor;

        // Die Daten der Aufzeichnung.
        schedule: Edit.ScheduleEditor;

        // Befehl zum Speichern der Aufzeichnung.
        readonly save = new JMSLib.App.Command(() => this.onSave(), "Übernehmen", () => this.isValid);

        // Befehl zum Löschen der Aufzeichnung.
        readonly del = new JMSLib.App.Command(() => this.onDelete(), "Löschen");

        // Meldet, ob alle Eingaben konsistent sind.
        private get isValid(): boolean {
            return this.job.isValid() && this.schedule.isValid();
        }

        // Gesetzt, wenn die Pflegeseite aus der Programmzeitschrift aufgerufen wurde.
        private _fromGuide: boolean;

        // Erstellt ein neues Präsentationsmodell.
        constructor(application: Application) {
            super("edit", application);

            // Eine neue Aufzeichnung kann von hier aus nicht mehr direkt angelegt werden.
            this.navigation.new = false;
        }

        // Initialisiert das Präsentationsmodell.
        reset(sections: string[]): void {
            // Zustand zurücksetzen.
            this.del.reset();
            this.save.reset();
            this.del.isDangerous = false;

            this.job = undefined;
            this.schedule = undefined;
            this._jobScheduleInfo = undefined;

            this._fromGuide = false;

            // Die Auswahlliste der Aufzeichnungsverzeichnisse.
            var folderSelection = [JMSLib.App.uiValue("", "(Voreinstellung verwenden)")];

            // Die Auswahlliste der Geräte.
            var profileSelection: JMSLib.App.IUiValue<string>[];

            // Zuerst die Liste der Aufzeichnungsverzeichnisse abfragen.
            VCRServer.RecordingDirectoryCache.getPromise().then(folders => {
                // Die möglichen Verzeichnisse anhängen.
                folderSelection.push(...folders.map(f => JMSLib.App.uiValue(f)));

                // Geräteprofile anfordern.
                return VCRServer.ProfileCache.getAllProfiles();
            }).then(profiles => {
                // Auswahl für den Anwender vorbereiten.
                profileSelection = profiles.map(p => JMSLib.App.uiValue(p.name));

                // Auf das Neuanlegen prüfen.
                if (sections.length > 0) {
                    // Auf existierende Aufzeichnung prüfen - wir gehen hier einfach mal von der Notation id= in der URL aus.
                    var id = sections[0].substr(3);
                    var epgId = (sections[1] || "epgid=").substr(6);

                    // Bei neuen Aufzeichnungen brauchen wir auch kein Löschen.
                    this.del.isVisible = (id !== "*");

                    // Einsprung aus der Programmzeitschrift.
                    this._fromGuide = !!epgId;

                    // Aufzeichnung asynchron abrufen - entweder existiert eine solche oder sie wird aus der Programmzeitschrift neu angelegt.
                    return VCRServer.createScheduleFromGuide(id, epgId);
                }

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

                // Beschreibung der Aufzeichnung vorbereiten.
                var info = <VCRServer.JobScheduleInfoContract>{
                    job: newJob,
                    jobId: null,
                    scheduleId: null,
                    schedule: this.createEmptySchedule()
                };

                // Die neue Aufzeichnung können wir auch direkt synchron bearbeiten.
                return info;
            }).then(info => this.setJobSchedule(info, profileSelection, folderSelection));
        }

        // Erstellt eine neue leere Aufzeichnung.
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
            // Liste der zuletzt verwendeten Quellen abrufen.
            var favorites = this.application.profile.recentSources || [];

            // Pflegemodelle anlegen.
            this._jobScheduleInfo = info;
            this.job = new Edit.JobEditor(this, info.job, profiles, favorites, folders, () => this.onChanged());
            this.schedule = new Edit.ScheduleEditor(this, info.schedule, favorites, () => this.onChanged(), () => (this.job.source.value || "").trim().length > 0);

            // Quellen für das aktuelle Geräteprofil laden und die Seite für den Anwender freigeben.
            this.loadSources().then(() => this.application.isBusy = false);
        }

        private loadSources(): JMSLib.App.IHttpPromise<VCRServer.SourceEntry[]> {
            var profile = this.job.device.value;

            return VCRServer.ProfileSourcesCache.getSources(profile).then(sources => {
                if (this.job.device.value === profile) {
                    this.job.source.sources = sources;
                    this.schedule.source.sources = sources;
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
            // Kopie der Aufzeichnungsdaten anlegen.
            var schedule = { ...this._jobScheduleInfo.schedule };

            // Dauer unter Berücksichtigung der Zeitumstellung anpassen.
            schedule.duration = JMSLib.App.DateTimeUtils.getRealDurationInMinutes(schedule.firstStart, schedule.duration);

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