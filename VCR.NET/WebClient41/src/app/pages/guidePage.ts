/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IGuidePage extends IPage {
    }

    export class GuidePage extends Page<JMSLib.App.ISite> implements IGuidePage {
        private _queryId = 0;

        private _profiles: VCRServer.ProfileInfoContract[];

        private _filter: VCRServer.GuideFilterContract =
        {
            cryptFilter: VCRServer.GuideEncryption.ALL,
            typeFilter: VCRServer.GuideSource.ALL,
            station: null,
            content: null,
            device: null,
            start: null,
            title: null,
            size: 20,
            index: 0
        };

        private _rows: GuideEntry[];

        get entries(): GuideEntry[] {
            return this._rows;
        }

        constructor(application: Application) {
            super("guide", application);

            // Navigation abweichend vom Standard konfigurieren.
            this.navigation.favorites = true;
            this.navigation.guide = false;
        }

        reset(section: string): void {
            this._queryId++;

            this._filter.size = this.application.profile.guideRows;

            VCRServer.ProfileCache.getPromise().then(profiles => {
                this._profiles = profiles || [];
                this._filter.device = this._profiles[0].name;

                this.query();
            });
        }

        get title(): string {
            return "Programmzeitschrift";
        }

        private query(): void {
            var queryId = ++this._queryId;

            VCRServer.queryProgramGuide(this._filter).then(items => {
                if (this._queryId !== queryId)
                    return;

                this._rows = (items || []).slice(0, this._filter.size).map(i => new GuideEntry(i));

                this.application.setBusy(false);
            });
        }
    }
}