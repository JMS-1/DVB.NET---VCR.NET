/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface ILogPage extends IPage {
        readonly profiles: JMSLib.App.IValidateStringFromList;

        readonly startDay: JMSLib.App.IValidateStringFromList;

        readonly showGuide: JMSLib.App.IValidatedFlag;

        readonly showScan: JMSLib.App.IValidatedFlag;

        readonly showLive: JMSLib.App.IValidatedFlag;

        readonly entries: ILogEntry[];
    }

    export class LogPage extends Page<JMSLib.App.ISite> implements ILogPage {

        readonly profiles = new JMSLib.App.EditStringFromList({}, "value", () => this.load(), "Protokollbereich", false, []);

        readonly showGuide = new JMSLib.App.EditFlag({}, "value", () => this.refreshUi(), "Programmzeitschrift");

        readonly showScan = new JMSLib.App.EditFlag({}, "value", () => this.refreshUi(), "Sendersuchlauf");

        readonly showLive = new JMSLib.App.EditFlag({}, "value", () => this.refreshUi(), "Zapping");

        private _startDay: string;

        readonly startDay: JMSLib.App.EditStringFromList;

        private _entries: LogEntry[] = [];

        get entries(): ILogEntry[] {
            return this._entries.filter(e => {
                if (e.isGuide)
                    return this.showGuide.value;
                if (e.isScan)
                    return this.showScan.value;
                if (e.isLive)
                    return this.showLive.value;

                return true;
            });
        }

        private _startup = false;

        private _requestId = 0;

        constructor(application: Application) {
            super("log", application);

            var now = new Date();
            var start = new Date(Date.UTC(now.getFullYear(), now.getMonth(), now.getDate()));
            var days: JMSLib.App.IUiValue<string>[] = [];

            for (var i = 0; i < 10; i++) {
                days.push({
                    display: JMSLib.DateFormatter.formatNumber(start.getUTCDate()) + '.' + JMSLib.DateFormatter.formatNumber(1 + start.getUTCMonth()),
                    value: start.toISOString()
                });

                start = new Date(Date.UTC(start.getUTCFullYear(), start.getUTCMonth(), start.getUTCDate() - 7));
            }

            this.startDay = new JMSLib.App.EditStringFromList(this, "_startDay", () => this.load(), null, false, days);
        }

        reset(sections: string[]): void {
            this._startup = true;
            this._requestId++;

            VCRServer.ProfileCache.getAllProfiles().then(list => this.setProfiles(list));
        }

        private setProfiles(profiles: VCRServer.ProfileInfoContract[]): void {
            this.profiles.allowedValues = profiles.map(p => <JMSLib.App.IUiValue<string>>{ display: p.name, value: p.name });
            this.profiles.value = profiles[0] && profiles[0].name;

            this.startDay.value = this.startDay.allowedValues[0].value;

            this._startup = false;
            this.load();
        }

        private load(): void {
            if (this._startup)
                return;

            var requestId = ++this._requestId;
            var profile = this.profiles.value;
            var endDay = new Date(this._startDay);
            var startDay = new Date(endDay.getTime() - 7 * 86400000);

            VCRServer.getProtocolEntries(profile, startDay, endDay).then(entries => {
                if (this._requestId !== requestId)
                    return;

                entries.reverse();

                var toggleDetail = this.toggleDetail.bind(this);

                this._entries = entries.map(e => new LogEntry(e, toggleDetail));

                this.application.setBusy(false);

                this.refreshUi();
            });
        }

        private toggleDetail(entry: LogEntry): void {
            var show = !entry.showDetail;

            this._entries.forEach(e => e.showDetail = false);

            entry.showDetail = show;

            this.refreshUi();
        }

        get title(): string {
            return `Aufzeichnungsprotokolle einsehen`;
        }
    }
}