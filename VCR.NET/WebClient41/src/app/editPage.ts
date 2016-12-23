/// <reference path="page.ts" />

namespace VCRNETClient.App {
    export class EditPage extends Page {
        job: JobData;

        schedule: ScheduleData;

        private _loadFinished = this.loadFinished.bind(this);

        private _loadPending: number;

        private _directories: string[];

        private _profiles: VCRServer.ProfileInfoContract[];

        private _sources: VCRServer.SourceEntry[];

        getName(): string {
            return "edit";
        }

        constructor(application: Application) {
            super(application);
        }

        reset(section: string): void {
            this._loadPending = 3;
            this._profiles = undefined;
            this._directories = undefined;

            VCRServer.RecordingDirectoryCache.load().then(dirs => this._directories = dirs).then(this._loadFinished);
            VCRServer.ProfileCache.load().then(profiles => this._profiles = profiles).then(this._loadFinished);
            VCRServer.createScheduleFromGuide(section.substr(3), "").then(info => {
                this.job = new JobData(info, null);
                this.schedule = new ScheduleData(info);

                // Quellen für das aktuelle Geräteprofil laden.
                return this.loadSources();
            }).then(this._loadFinished);
        }

        private loadFinished(): void {
            if (--this._loadPending === 0)
                this.application.setBusy(false);
        }

        private loadSources(): Thenable<VCRServer.SourceEntry[]> {
            var profile = this.job.device;

            return VCRServer.ProfileSourcesCache.load(profile).then(sources => {
                if (this.job.device === profile)
                    this._sources = sources;

                return sources;
            });
        }

        showNew(): boolean {
            return false;
        }

        getTitle(): string {
            return `[TBD]`;
        }
    }
}