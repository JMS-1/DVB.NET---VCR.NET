/// <reference path="section.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminRulesPage extends ISection {
        readonly rules: JMSLib.App.IString;
    }

    export class RulesSection extends Section implements IAdminRulesPage {

        readonly rules = new JMSLib.App.String({}, "rules");

        protected loadAsync(): void {
            VCRServer.getSchedulerRules().then(settings => this.initialize(settings));
        }

        private initialize(settings: VCRServer.SchedulerRulesContract): void {
            this.rules.data = settings;

            this.page.application.isBusy = false;
        }

        protected readonly saveCaption = "Ändern und neu Starten";

        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            return VCRServer.setSchedulerRules(this.rules.data);
        }
     }
}