/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IAdminSecurityPage extends IPage {
    }

    export interface IAdminDirectoriesPage extends IPage {
    }

    export interface IAdminDevicesPage extends IPage {
    }

    export interface IAdminGuidePage extends IPage {
    }

    export interface IAdminSourcesPage extends IPage {
    }

    export interface IAdminRulesPage extends IPage {
    }

    export interface IAdminOtherPage extends IPage {
    }

    export interface IAdminPage extends IPage {
        readonly section: string;

        readonly sections: string[];

        readonly sectionNames: { readonly[section: string]: string; };
    }

    export class AdminPage extends Page<JMSLib.App.ISite> implements IAdminPage, IAdminSecurityPage, IAdminDirectoriesPage, IAdminDevicesPage, IAdminGuidePage, IAdminSourcesPage, IAdminRulesPage, IAdminOtherPage {

        private static readonly _sections: string[] = [
            "security",
            "directories",
            "devices",
            "guide",
            "sources",
            "rules",
            "other",
        ];

        private static readonly _sectionNames: { [section: string]: string; } = {
            directories: "Verzeichnisse",
            guide: "Programmzeitschrift",
            rules: "Planungsregeln",
            security: "Sicherheit",
            other: "Sonstiges",
            sources: "Quellen",
            devices: "Geräte",
        };

        constructor(application: Application) {
            super("admin", application);
        }

        private _section: string;

        get section(): string {
            return this._section || AdminPage._sections[0];
        }

        get sections(): string[] {
            return AdminPage._sections;
        }

        get sectionNames(): { [section: string]: string; } {
            return AdminPage._sectionNames;
        }

        reset(sections: string[]): void {
            this._section = sections[0];

            window.setTimeout(() => this.application.setBusy(false), 0);
        }

        get title(): string {
            return `Administration und Konfiguration`;
        }
    }
}