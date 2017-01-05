/// <reference path="noUi/page.ts" />

namespace VCRNETClient.App {
    export interface IHelpSite extends NoUi.INoUiSite {
        getCurrentHelpTitle(section: string): string;
    }

    export class HelpPage extends NoUi.Page<IHelpSite> {
        getRoute(): string {
            return "faq";
        }

        section: string;

        constructor(application: Application) {
            super(application);
        }

        reset(section: string): void {
            this.section = section;

            setTimeout(() => this.application.setBusy(false), 0);
        }

        getTitle(): string {
            var site = this.getSite();

            return site && site.getCurrentHelpTitle(this.section);
        }
    }
}