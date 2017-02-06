/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminGuidePage extends IAdminSection {
        readonly hours: JMSLib.App.IMultiValueFromList<number>;

        readonly sources: JMSLib.App.IMultiValueFromList<string>;

        readonly remove: JMSLib.App.ICommand;
    }

    export class GuideSection extends AdminSection implements IAdminGuidePage {

        readonly hours = new JMSLib.App.SelectFromList<number>({}, "hours", null, "Uhrzeiten", AdminPage.hoursOfDay);

        readonly sources = new JMSLib.App.SelectFromList<string>({ list: [] }, "list", () => this.refreshUi(), null, []);

        readonly remove = new JMSLib.App.Command(() => this.removeSources(), "Entfernen", () => this.sources.value.length > 0);

        reset(): void {
            VCRServer.getGuideSettings().then(settings => this.setSettings(settings));
        }

        private setSettings(settings: VCRServer.GuideSettingsContract): void {
            this.hours.data = settings;

            this.sources.setValues(settings.sources.map(s => <JMSLib.App.IUiValue<string>>{ value: s, display: s }));
            this.sources.value = [];

            this.page.application.setBusy(false);

            this.refreshUi();
        }

        private removeSources(): void {
            var settings = <VCRServer.GuideSettingsContract>this.hours.data;

            this.sources.removeSelected();

            settings.sources = this.sources.allValues;
        }
    }
}