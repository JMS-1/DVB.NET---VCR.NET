/// <reference path="page.ts" />
/// <reference path="../../lib/dateTimeFormatter.ts" />
/// <reference path="../../lib/edit/list.ts" />

namespace VCRNETClient.App {

    class SectionInfo implements Admin.ISectionInfo<Admin.ISection> {
        constructor(private readonly _factory: { new (page: AdminPage): Admin.ISection; readonly sectionName: string; }, private readonly _adminPage: AdminPage) {
        }

        private _page: Admin.ISection;

        get page(): Admin.ISection {
            if (!this._page)
                this._page = new (this._factory)(this._adminPage);

            return this._page;
        }

        get display(): string {
            return this._factory.sectionName;
        }
    }

    export interface IAdminPage extends IPage {
        readonly section: string;

        readonly sectionNames: string[];

        readonly sections: Admin.ISectionInfos<Admin.ISectionInfo<any>>;
    }

    export class AdminPage extends Page implements IAdminPage {

        static readonly hoursOfDay = (() => {
            var hours: JMSLib.App.IUiValue<number>[] = [];

            for (var i = 0; i < 24; i++)
                hours.push(JMSLib.App.uiValue(i, JMSLib.App.DateFormatter.formatNumber(i)));

            return hours;
        })();

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

        readonly sections = {
            directories: new SectionInfo(Admin.DirectoriesSection, this),
            security: new SectionInfo(Admin.SecuritySection, this),
            devices: new SectionInfo(Admin.DevicesSection, this),
            sources: new SectionInfo(Admin.ScanSection, this),
            guide: new SectionInfo(Admin.GuideSection, this),
            other: new SectionInfo(Admin.OtherSection, this),
            rules: new SectionInfo(Admin.RulesSection, this),
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

        update<TResponseType>(promise: JMSLib.App.IHttpPromise<boolean>, command: JMSLib.App.Command<TResponseType>): JMSLib.App.IHttpPromise<void> {
            command.message = ``;

            return promise.then(restartRequired => {
                if (restartRequired === true)
                    alert(`RESTART`);
                else if (restartRequired !== false)
                    command.message = `Ausführung zurzeit nicht möglich`;
                else
                    this.application.gotoPage(null);
            }, error => {
                command.message = error.message;

                return error;
            });
        }

        get title(): string {
            return `Administration und Konfiguration`;
        }
    }
}