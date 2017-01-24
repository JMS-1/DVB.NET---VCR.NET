/// <reference path="page.ts" />
/// <reference path="../../../scripts/VCRServer.ts" />

namespace VCRNETClient.App {

    export interface IGuidePageNavigation {
        readonly firstPage: JMSLib.App.ICommand;

        readonly prevPage: JMSLib.App.ICommand;

        readonly nextPage: JMSLib.App.ICommand;

        readonly profiles: JMSLib.App.IValidateStringFromList;

        readonly sources: JMSLib.App.IValidateStringFromList;

        readonly encrpytion: JMSLib.App.IValueFromList<VCRServer.GuideEncryption>;

        readonly sourceType: JMSLib.App.IValueFromList<VCRServer.GuideSource>;
    }

    export interface IGuidePage extends IPage, IGuidePageNavigation {
        readonly entries: GuideEntry[];
    }

    export class GuidePage extends Page<JMSLib.App.ISite> implements IGuidePage {
        private static _cryptOptions: JMSLib.App.IUiValue<VCRServer.GuideEncryption>[] = [
            { display: "Nur unverschlüsselt", value: VCRServer.GuideEncryption.FREE },
            { display: "Nur verschlüsselt", value: VCRServer.GuideEncryption.PAY },
            { display: "Alle Quellen", value: VCRServer.GuideEncryption.ALL }
        ];

        private static _typeOptions: JMSLib.App.IUiValue<VCRServer.GuideSource>[] = [
            { display: "Nur Fernsehen", value: VCRServer.GuideSource.TV },
            { display: "Nur Radio", value: VCRServer.GuideSource.RADIO },
            { display: "Alle Quellen", value: VCRServer.GuideSource.ALL }
        ];

        private _queryId = 0;

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

        readonly profiles = new JMSLib.App.EditStringFromList(this._filter, "device", () => this.onDeviceChanged(true), "Gerät", false, []);

        readonly sources = new JMSLib.App.EditStringFromList(this._filter, "station", () => this.query(), "Quelle", false, []);

        readonly encrpytion = new JMSLib.App.EditFromList<VCRServer.GuideEncryption>(this._filter, "cryptFilter", () => this.query(), null, GuidePage._cryptOptions);

        readonly sourceType = new JMSLib.App.EditFromList<VCRServer.GuideSource>(this._filter, "typeFilter", () => this.query(), null, GuidePage._typeOptions);

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
                this.profiles.allowedValues = (profiles || []).map(p => <JMSLib.App.IUiValue<string>>{ display: p.name, value: p.name });

                this._filter.device = this.profiles.allowedValues[0].value;

                this.onDeviceChanged(false);
            });
        }

        get title(): string {
            return "Programmzeitschrift";
        }

        private resetFilter(): void {
            this._filter.cryptFilter = VCRServer.GuideEncryption.ALL;
            this._filter.typeFilter = VCRServer.GuideSource.ALL;
            this._filter.station = null;
            this._filter.content = null;
            this._filter.start = null;
            this._filter.title = null;
            this._filter.index = 0;
        }

        private onDeviceChanged(resetFilter: boolean) {
            VCRServer.GuideInfoCache.getPromise(this._filter.device).then(info => {
                var sources = (info.stations || []).map(s => <JMSLib.App.IUiValue<string>>{ display: s, value: s });

                sources.unshift({ display: "(Alle Sender)", value: null });

                this.sources.allowedValues = sources;

                if (resetFilter)
                    this.resetFilter();

                this.query();
            });
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