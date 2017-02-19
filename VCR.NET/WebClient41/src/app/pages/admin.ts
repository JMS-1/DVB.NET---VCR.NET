/// <reference path="page.ts" />
/// <reference path="../../lib/edit/list.ts" />
/// <reference path="../../lib/dateTimeFormatter.ts" />

namespace VCRNETClient.App {

    // Interne Verwaltungseinheit für Konfigurationsbereiche.
    class SectionInfo implements Admin.ISectionInfo<Admin.ISection> {

        // Erstellt eine neue Verwaltung.
        constructor(public readonly route: string, private readonly _factory: { new (page: AdminPage): Admin.Section<any> }, private readonly _adminPage: AdminPage) {
        }

        // Die Präsentationsinstanz des zugehörigen Konfigurationsbereichs.
        private _page: Admin.Section<any>;

        // Meldet die Präsentation des zugehörigen Konfigurationsbereichs - bei Bedarf wird eine neue erstellt.
        get page(): Admin.Section<any> {
            // Beim ersten Aufruf eine neue Präsentationsinstanz anlegen.
            if (!this._page)
                this._page = new (this._factory)(this._adminPage);

            return this._page;
        }
    }

    // Schnittstelle zur Anzeige der Administration.
    export interface IAdminPage extends IPage {
        readonly sections: JMSLib.App.IValueFromList<Admin.ISectionInfo<any>>;
    }

    export class AdminPage extends Page implements IAdminPage {

        static readonly hoursOfDay = (() => {
            var hours: JMSLib.App.IUiValue<number>[] = [];

            for (var i = 0; i < 24; i++)
                hours.push(JMSLib.App.uiValue(i, JMSLib.App.DateFormatter.formatNumber(i)));

            return hours;
        })();

        private static _windowsGroups: JMSLib.App.IHttpPromise<JMSLib.App.IUiValue<string>[]>;

        private readonly _sections: JMSLib.App.IUiValue<SectionInfo>[] = [
            JMSLib.App.uiValue(new SectionInfo("security", Admin.SecuritySection, this), "Sicherheit"),
            JMSLib.App.uiValue(new SectionInfo("directories", Admin.DirectoriesSection, this), "Verzeichnisse"),
            JMSLib.App.uiValue(new SectionInfo("devices", Admin.DevicesSection, this), "Geräte"),
            JMSLib.App.uiValue(new SectionInfo("guide", Admin.GuideSection, this), "Programmzeitschrift"),
            JMSLib.App.uiValue(new SectionInfo("scan", Admin.ScanSection, this), "Quellen"),
            JMSLib.App.uiValue(new SectionInfo("rules", Admin.RulesSection, this), "Planungsregeln"),
            JMSLib.App.uiValue(new SectionInfo("other", Admin.OtherSection, this), "Sonstiges")
        ];

        readonly sections: JMSLib.App.SelectSingleFromList<SectionInfo> = new JMSLib.App.SelectSingleFromList<SectionInfo>({}, "value", null, null, false, this._sections);

        constructor(application: Application) {
            super("admin", application);
        }

        reset(sections: string[]): void {
            // Melden, dass alle ausstehenden asynchronen Anfragen von nun an nicht mehr interessieren.
            JMSLib.App.switchView();

            var allSections = this.sections.allowedValues;
            var curSection = allSections.filter(v => v.value.route === sections[0])[0] || allSections[0];

            this.sections.value = curSection.value;

            curSection.value.page.reset();
        }

        update<TResponseType>(promise: JMSLib.App.IHttpPromise<boolean>, command: JMSLib.App.Command<TResponseType>): JMSLib.App.IHttpPromise<void> {
            command.message = ``;

            return promise.then(restartRequired => {
                if (restartRequired === true)
                    alert(`RESTART`);
                else if (restartRequired !== false)
                {
                    command.message = `Ausführung zurzeit nicht möglich`;

                    this.application.restart();
                }
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