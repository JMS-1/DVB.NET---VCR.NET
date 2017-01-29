/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface ILogPage extends IPage {
        readonly profiles: JMSLib.App.IValidateStringFromList;
    }

    export class LogPage extends Page<JMSLib.App.ISite> implements ILogPage {

        readonly profiles = new JMSLib.App.EditStringFromList({}, "value", () => this.load(), "Protokollbereich", false, []);

        constructor(application: Application) {
            super("log", application);
        }

        reset(sections: string[]): void {
            VCRServer.ProfileCache.getAllProfiles().then(list => this.setProfiles(list));
        }

        private setProfiles(profiles: VCRServer.ProfileInfoContract[]): void {
            this.profiles.allowedValues = profiles.map(p => <JMSLib.App.IUiValue<string>>{ display: p.name, value: p.name });
            this.profiles.value = profiles[0] && profiles[0].name;

            this.application.setBusy(false);
        }

        private load(): void {
        }

        get title(): string {
            return `Aufzeichnungsprotokolle einsehen`;
        }
    }
}