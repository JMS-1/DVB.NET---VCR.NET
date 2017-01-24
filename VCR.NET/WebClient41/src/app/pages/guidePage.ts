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

        readonly showEncryption: boolean;

        readonly sourceType: JMSLib.App.IValueFromList<VCRServer.GuideSource>;

        readonly showSourceType: boolean;

        readonly days: JMSLib.App.IValidateStringFromList;

        readonly hours: JMSLib.App.IValueFromList<number>;
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

        private static _hours: JMSLib.App.IUiValue<number>[] = [
            { display: "00:00", value: 0 },
            { display: "06:00", value: 6 },
            { display: "12:00", value: 12 },
            { display: "18:00", value: 18 },
            { display: "20:00", value: 20 },
            { display: "22:00", value: 22 },
        ];

        private _queryId = 0;

        private _filter: VCRServer.GuideFilterContract =
        {
            cryptFilter: VCRServer.GuideEncryption.ALL,
            typeFilter: VCRServer.GuideSource.ALL,
            content: null,
            device: null,
            start: null,
            title: null,
            station: "",
            size: 20,
            index: 0
        };

        readonly profiles = new JMSLib.App.EditStringFromList(this._filter, "device", () => this.onDeviceChanged(true), "Gerät", false, []);

        readonly sources = new JMSLib.App.EditStringFromList(this._filter, "station", () => this.query(), "Quelle", false, []);

        readonly encrpytion = new JMSLib.App.EditFromList<VCRServer.GuideEncryption>(this._filter, "cryptFilter", () => this.query(), null, GuidePage._cryptOptions);

        readonly sourceType = new JMSLib.App.EditFromList<VCRServer.GuideSource>(this._filter, "typeFilter", () => this.query(), null, GuidePage._typeOptions);

        readonly days = new JMSLib.App.EditStringFromList(this._filter, "start", () => this.query(), "Datum", false, []);

        private _hour = -1;

        readonly hours = new JMSLib.App.EditFromList<number>(this, "_hour", () => this.query(), "Start ab", GuidePage._hours);

        readonly firstPage = new JMSLib.App.Command(() => this.changePage(-this._filter.index), "Erste Seite", () => this._filter.index > 0);

        readonly prevPage = new JMSLib.App.Command(() => this.changePage(-1), "Vorherige Seite", () => this._filter.index > 0);

        readonly nextPage = new JMSLib.App.Command(() => this.changePage(+1), "Nächste Seite", () => this._hasMore);

        get showEncryption(): boolean {
            return !this._filter.station;
        }

        get showSourceType(): boolean {
            return this.showEncryption;
        }

        entries: GuideEntry[] = [];

        private _hasMore = false;

        private _profileInfo: VCRServer.GuideInfoContract;

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
            this._filter.content = null;
            this._filter.station = "";
            this._filter.start = null;
            this._filter.title = null;
            this._filter.index = 0;
            this._hour = -1;
        }

        private onDeviceChanged(resetFilter: boolean) {
            VCRServer.GuideInfoCache.getPromise(this._filter.device).then(info => {
                this._profileInfo = info;

                this.refreshSources();
                this.refreshDays();

                if (resetFilter)
                    this.resetFilter();

                this.query();
            });
        }

        private refreshSources(): void {
            var sources = (this._profileInfo.stations || []).map(s => <JMSLib.App.IUiValue<string>>{ display: s, value: s });

            sources.unshift({ display: "(Alle Sender)", value: "" });

            this.sources.allowedValues = sources;
        }

        private refreshDays(): void {
            var days: JMSLib.App.IUiValue<string>[] = [];

            if (this._profileInfo.first && this._profileInfo.last) {
                days.push({ display: "Jetzt", value: null });

                var firstUtc = new Date(this._profileInfo.first);
                var first = new Date(firstUtc.getUTCFullYear(), firstUtc.getUTCMonth(), firstUtc.getUTCDate());

                var lastUtc = new Date(this._profileInfo.last);
                var last = new Date(lastUtc.getUTCFullYear(), lastUtc.getUTCMonth(), lastUtc.getUTCDate());

                for (var i = 0; (i < 14) && (first.getTime() <= last.getTime()); i++) {
                    var display = JMSLib.App.DateFormatter.getShortDate(first);
                    var value = first.toISOString();

                    days.push({ display: display, value: value });

                    first = new Date(first.getTime() + 86400000);
                }
            }

            this.days.allowedValues = days;
        }

        private query(): void {
            var queryId = ++this._queryId;

            if (this._hour >= 0) {
                var start = this._filter.start ? new Date(this._filter.start) : new Date();

                this._filter.start = new Date(start.getFullYear(), start.getMonth(), start.getDate(), this._hour).toISOString();
                this._hour = -1;
            }

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