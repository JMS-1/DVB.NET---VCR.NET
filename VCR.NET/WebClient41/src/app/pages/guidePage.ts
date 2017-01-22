/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IGuidePageNavigation {
        readonly firstPage: JMSLib.App.ICommand;

        readonly prevPage: JMSLib.App.ICommand;

        readonly nextPage: JMSLib.App.ICommand;
    }

    export interface IGuidePage extends IPage, IGuidePageNavigation {
        readonly entries: GuideEntry[];
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

        readonly firstPage = new JMSLib.App.Command(() => this.changePage(-this._filter.index), "Erste Seite", () => this._filter.index > 0);

        readonly prevPage = new JMSLib.App.Command(() => this.changePage(-1), "Vorherige Seite", () => this._filter.index > 0);

        readonly nextPage = new JMSLib.App.Command(() => this.changePage(+1), "Nächste Seite", () => this._hasMore);

        entries: GuideEntry[] = [];

        private _hasMore = false;

        constructor(application: Application) {
            super("guide", application);

            // Navigation abweichend vom Standard konfigurieren.
            this.navigation.favorites = true;
            this.navigation.guide = false;
        }

        reset(section: string): void {
            this._queryId++;
            this.entries = [];
            this._hasMore = false;

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

                this.entries = (items || []).slice(0, this._filter.size).map(i => new GuideEntry(i));
                this._hasMore = items && (items.length > this._filter.size);

                this.application.setBusy(false);

                this.refreshUi();
            });
        }

        private changePage(delta: number): JMSLib.App.Thenable<void, XMLHttpRequest> {
            this._filter.index += delta;

            this.query();

            return new JMSLib.App.Promise<void, XMLHttpRequest>(success => success(undefined));
        }
    }
}