/// <reference path="noUi/page.ts" />

namespace VCRNETClient.App {
    export interface IHelpSite extends NoUi.INoUiSite {
        getCurrentHelpTitle(section: string): string;
    }

    export class HelpPage extends NoUi.Page<IHelpSite> {
        constructor(application: Application) {
            super("faq", application);
        }

        section: string;

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