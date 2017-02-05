/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IAdminSection extends JMSLib.App.IConnectable {
        readonly page: IAdminPage;
    }

    export abstract class AdminSection implements IAdminSection {

        constructor(public readonly page: AdminPage) {
        }

        site: JMSLib.App.ISite;

        protected refreshUi(): void {
            if (this.site)
                this.site.refreshUi();
        }

        abstract reset(): void;
    }

    export interface IAdminGuidePage extends IPage {
    }

    export interface IAdminSourcesPage extends IPage {
    }

    export interface IAdminRulesPage extends IPage {
    }

    export interface IAdminOtherPage extends IPage {
    }

    export interface IAdminSectionInfo<TPageType extends IAdminSection> {
        readonly display: string;

        readonly page: TPageType;
    }

    interface IInternalAdminSectionInfo<TPageType extends IAdminSection> extends IAdminSectionInfo<TPageType> {
        readonly factory: { new (page: AdminPage): AdminSection };

        page: TPageType;
    }

    export interface IAdminSectionInfos<TInterface extends IAdminSectionInfo<any>> {
        readonly[section: string]: TInterface;
    }

    interface IInternalAdminSectionInfos extends IAdminSectionInfos<IInternalAdminSectionInfo<any>> {
    }

    export interface IAdminPage extends IPage {
        readonly section: string;

        readonly sectionNames: string[];

        readonly sections: IAdminSectionInfos<IAdminSectionInfo<any>>;
    }

    export class AdminPage extends Page implements IAdminPage {

        private static _windowsGroups: JMSLib.App.IHttpPromise<JMSLib.App.IUiValue<string>[]>;

        private static readonly _sectionNames: string[] = [
            "security",
            "directories",
            "devices",
            "guide",
            "sources",
            "rules",
            "other",
        ];

        readonly sections: IInternalAdminSectionInfos = {
            directories: { display: "Verzeichnisse", page: null, factory: Admin.DirectoriesSection },
            security: { display: "Sicherheit", page: null, factory: Admin.SecuritySection },
            devices: { display: "Geräte", page: null, factory: Admin.DevicesSection },
            guide: { display: "Programmzeitschrift", page: null, factory: null },
            rules: { display: "Planungsregeln", page: null, factory: null },
            other: { display: "Sonstiges", page: null, factory: null },
            sources: { display: "Quellen", page: null, factory: null },
        };

        constructor(application: Application) {
            super("admin", application);
        }

        private _section: string;

        get section(): string {
            return this._section || AdminPage._sectionNames[0];
        }

        get sectionNames(): string[] {
            return AdminPage._sectionNames;
        }

        reset(sections: string[]): void {
            this._section = sections[0];

            var sectionInfo = this.sections[this.section];

            if (!sectionInfo)
                return;

            if (!sectionInfo.page && sectionInfo.factory)
                sectionInfo.page = new sectionInfo.factory(this);

            if (sectionInfo.page)
                sectionInfo.page.reset();
        }

        update(promise: JMSLib.App.IHttpPromise<boolean>): JMSLib.App.IHttpPromise<void> {
            return promise.then(restartRequired => {
                if (restartRequired === true)
                    alert(`RESTART`);
                else if (restartRequired === false)
                    alert(`FAILED`);

                this.application.gotoPage(null);
            });
        }

        get title(): string {
            return `Administration und Konfiguration`;
        }
    }
}