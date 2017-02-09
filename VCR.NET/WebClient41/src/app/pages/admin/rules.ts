/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminRulesPage extends IAdminSection {
        readonly rules: JMSLib.App.IValidatedString;

        readonly update: JMSLib.App.ICommand;
    }

    export class RulesSection extends AdminSection implements IAdminRulesPage {

        readonly rules = new JMSLib.App.EditString({}, "rules", null, null, false);

        readonly update = new JMSLib.App.Command(() => this.save(), "Ändern und neu Starten");

        reset(): void {
            this.update.message = ``;

            VCRServer.getSchedulerRules().then(settings => this.setSettings(settings));
        }

        private setSettings(settings: VCRServer.SchedulerRulesContract): void {
            this.rules.data = settings;

            this.page.application.isBusy = false;
        }

        private save(): JMSLib.App.IHttpPromise<void> {
            var settings = <VCRServer.SchedulerRulesContract>this.rules.data;

            return this.page.update(VCRServer.setSchedulerRules(settings), this.update);
        }
     }
}