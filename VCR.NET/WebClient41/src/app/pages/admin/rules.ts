/// <reference path="section.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminRulesPage extends ISection {
        readonly rules: JMSLib.App.IEditString;
    }

    export class RulesSection extends Section<VCRServer.SchedulerRulesContract> implements IAdminRulesPage {

        readonly rules = new JMSLib.App.EditString({}, "rules", null, null, false);

        protected loadAsync(): JMSLib.App.IHttpPromise<VCRServer.SchedulerRulesContract> {
            return VCRServer.getSchedulerRules();
        }

        protected initialize(settings: VCRServer.SchedulerRulesContract): void {
            this.rules.data = settings;

            this.page.application.isBusy = false;
        }

        protected readonly saveCaption = "Ändern und neu Starten";

        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            return VCRServer.setSchedulerRules(this.rules.data);
        }
     }
}