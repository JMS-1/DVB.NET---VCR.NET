/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IAdminSecurityPage extends IPage {
        readonly userGroups: JMSLib.App.IValidateStringFromList;

        readonly adminGroups: JMSLib.App.IValidateStringFromList;

        readonly saveSecurity: JMSLib.App.ICommand;
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

    export interface IAdminSectionInfo<TPageType extends IPage> {
        readonly display: string;

        readonly page: TPageType;
    }

    interface IInternalAdminSectionInfo extends IAdminSectionInfo<any> {
        readonly init: () => void;
    }

    export interface IAdminSectionInfos {
        readonly[section: string]: IAdminSectionInfo<any>;
    }

    interface IInternalAdminSectionInfos extends IAdminSectionInfos {
        readonly[section: string]: IInternalAdminSectionInfo;
    }

    export interface IAdminPage extends IPage {
        readonly section: string;

        readonly sectionNames: string[];

        readonly sections: IAdminSectionInfos;
    }

    export class AdminPage extends Page<JMSLib.App.ISite> implements IAdminPage {

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

        private readonly _sections: IInternalAdminSectionInfos = {
            security: { display: "Sicherheit", page: this, init: this.initSecurity },
            directories: { display: "Verzeichnisse", page: this, init: null },
            guide: { display: "Programmzeitschrift", page: this, init: null },
            rules: { display: "Planungsregeln", page: this, init: null },
            other: { display: "Sonstiges", page: this, init: null },
            sources: { display: "Quellen", page: this, init: null },
            devices: { display: "Geräte", page: this, init: null },
        };

        readonly userGroups = new JMSLib.App.EditStringFromList({}, "value", null, "Benutzer:", false, []);

        readonly adminGroups = new JMSLib.App.EditStringFromList({}, "value", null, "Administratoren:", false, []);

        readonly saveSecurity = new JMSLib.App.Command(() => this.updateSecurity(), "Ändern");

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

        get sections(): IAdminSectionInfos {
            return this._sections;
        }

        reset(sections: string[]): void {
            this._section = sections[0];

            var sectionInfo = this._sections[this.section];

            if (sectionInfo && sectionInfo.init)
                sectionInfo.init.bind(this)();
        }

        private update(promise: JMSLib.App.IHttpPromise<boolean>): void {
            promise.then(restartRequired => this.application.gotoPage(null));
        }

        private initSecurity(): void {
            if (!AdminPage._windowsGroups)
                AdminPage._windowsGroups = VCRServer.getWindowsGroups().then(names => {
                    var groups = [<JMSLib.App.IUiValue<string>>{ display: "(Alle Benutzer)", value: "" }];

                    groups.push(...names.map(name => <JMSLib.App.IUiValue<string>>{ display: name, value: name }));

                    return groups;
                });

            AdminPage._windowsGroups.then(groups => this.setWindowsGroups(groups));
        }

        private updateSecurity(): void {
            var settings: VCRServer.SecuritySettingsContract = {
                users: this.userGroups.value,
                admins: this.adminGroups.value
            };

            this.update(VCRServer.setSecuritySettings(settings));
        }

        private setWindowsGroups(groups: JMSLib.App.IUiValue<string>[]): void {
            this.userGroups.allowedValues = groups;
            this.adminGroups.allowedValues = groups;

            VCRServer.getSecuritySettings().then(settings => this.setSecurity(settings));
        }

        private setSecurity(security: VCRServer.SecuritySettingsContract): void {
            this.userGroups.value = security.users;
            this.adminGroups.value = security.admins;

            this.application.setBusy(false);
        }

        get title(): string {
            return `Administration und Konfiguration`;
        }
    }
}