/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IHomePage extends IPage {
        readonly startGuide: JMSLib.App.ICommand;

        readonly showStartGuide: JMSLib.App.IFlag;

        readonly startScan: JMSLib.App.ICommand;

        readonly showStartScan: JMSLib.App.IFlag;

        readonly isRecording: boolean;

        readonly checkVersion: JMSLib.App.IFlag;

        readonly currentVersion: string;

        readonly onlineVersion: string;

        readonly versionMismatch: boolean;
    }

    // Die Anwendungslogik für die Startseite.
    export class HomePage extends Page implements IHomePage {

        private static _versionExtract = />VCRNET\.MSI<\/a>[^<]*\s([^\s]+)\s*</i;

        readonly startGuide = new JMSLib.App.Command(() => this.startTask(`guideUpdate`), "Aktualisierung anfordern", () => this.application.version.hasGuides);

        readonly showStartGuide = new JMSLib.App.Flag({}, "value", "die Programmzeitschrift sobald wie möglich aktualisieren", () => this.refreshUi(), () => !this.application.version.hasGuides);

        readonly startScan = new JMSLib.App.Command(() => this.startTask(`sourceScan`), "Aktualisierung anfordern", () => this.application.version.canScan);

        readonly showStartScan = new JMSLib.App.Flag({}, "value", "einen Sendersuchlauf sobald wie möglich durchführen", () => this.refreshUi(), () => !this.application.version.canScan);

        readonly checkVersion = new JMSLib.App.Flag({}, "value", "neue Version", () => this.toggleVersionCheck());

        // Erstellt die Anwendungslogik.
        constructor(application: Application) {
            super("home", application);

            // Meldet, dass die Navigationsleiste nicht angezeigt werden soll.
            this.navigation = null;
        }

        get currentVersion(): string {
            return this.application.version.msiVersion;
        }

        private _onlineVersion: string;

        get onlineVersion(): string {
            return this._onlineVersion || `(wird ermittelt)`;
        }

        get versionMismatch(): boolean {
            return this._onlineVersion && (this._onlineVersion !== this.currentVersion);
        }

        private toggleVersionCheck(): void {
            if (this.checkVersion.value) {
                this._onlineVersion = undefined;

                VCRServer.doUrlCall(`http://downloads.psimarron.net`).then((html: string) => {
                    var match = HomePage._versionExtract.exec(html);
                    if (match == null)
                        return;
                    if (match.length < 2)
                        return;

                    this._onlineVersion = match[1];

                    this.refreshUi();
                });
            }

            this.refreshUi();
        }

        // Zeigt die Startseite (erneut) an.
        reset(sections: string[]): void {
            this._onlineVersion = undefined;

            this.startScan.reset();
            this.startGuide.reset();
            this.checkVersion.value = false;
            this.showStartScan.value = false;
            this.showStartGuide.value = false;

            this.application.isBusy = false;
        }

        // Meldet die Überschrift der Startseite.
        get title(): string {
            // Der Titel wird dem aktuellen Kenntnisstand angepasst.
            var version = this.application.version;
            var title = this.application.title;

            if (version)
                return `${title} (${version.msiVersion})`;
            else
                return title;
        }

        get isRecording(): boolean {
            return this.application.version.active;
        }

        private startTask(task: string): JMSLib.App.IHttpPromise<void> {
            return VCRServer.triggerTask(task).then(() => this.application.gotoPage(this.application.devicesPage.route));
        }
    }
}