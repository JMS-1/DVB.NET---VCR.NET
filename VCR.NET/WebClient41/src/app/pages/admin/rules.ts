/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminRulesPage extends IAdminSection {
        readonly rules: JMSLib.App.IValidatedString;
    }

    export class RulesSection extends AdminSection<VCRServer.SchedulerRulesContract> implements IAdminRulesPage {

        readonly rules = new JMSLib.App.EditString({}, "rules", null, null, false);

        reset(): void {
            this.update.message = ``;

            VCRServer.getSchedulerRules().then(settings => this.setSettings(settings));
        }

        private setSettings(settings: VCRServer.SchedulerRulesContract): void {
            this.rules.data = settings;

            this.page.application.isBusy = false;
        }

        protected readonly saveCaption = "Ändern und neu Starten";

        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            return VCRServer.setSchedulerRules(this.rules.data);
        }
     }
}