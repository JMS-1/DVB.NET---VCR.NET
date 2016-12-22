/// <reference path="page.ts" />

namespace VCRNETClient.App {
    export interface IHelpSite {
        getCurrentHelpTitle(section: string): string;
    }

    export class HelpPage extends Page {
        getName(): string {
            return "faq";
        }

        section: string;

        private _site: IHelpSite;

        constructor(application: Application) {
            super(application);
        }

        setSite(site: IHelpSite): void {
            if (this._site !== site)
                this._site = site;
        }

        reset(section: string): void {
            this.section = section;

            setTimeout(() => this.application.setBusy(false), 0);
        }

        getTitle(): string {
            return this._site && this._site.getCurrentHelpTitle(this.section);
        }
    }
}