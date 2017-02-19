/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IHomePage extends IPage {
        readonly startGuide: JMSLib.App.ICommand;

        readonly showStartGuide: JMSLib.App.IFlag;

        readonly startScan: JMSLib.App.ICommand;

        readonly showStartScan: JMSLib.App.IFlag;

        readonly isRecording: boolean;
    }

    // Die Anwendungslogik für die Startseite.
    export class HomePage extends Page implements IHomePage {

        readonly startGuide = new JMSLib.App.Command(() => this.startTask(`guideUpdate`), "Aktualisierung anfordern", () => this.application.version.hasGuides);

        readonly showStartGuide = new JMSLib.App.Flag({}, "value", "die Programmzeitschrift sobald wie möglich aktualisieren", () => this.refreshUi(), () => !this.application.version.hasGuides);

        readonly startScan = new JMSLib.App.Command(() => this.startTask(`sourceScan`), "Aktualisierung anfordern", () => this.application.version.canScan);

        readonly showStartScan = new JMSLib.App.Flag({}, "value", "einen Sendersuchlauf sobald wie möglich durchführen", () => this.refreshUi(), () => !this.application.version.canScan);

        // Erstellt die Anwendungslogik.
        constructor(application: Application) {
            super("home", application);

            // Meldet, dass die Navigationsleiste nicht angezeigt werden soll.
            this.navigation = null;
        }

        // Zeigt die Startseite (erneut) an.
        reset(sections: string[]): void {
            this.startScan.reset();
            this.startGuide.reset();
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