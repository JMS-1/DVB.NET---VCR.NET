/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminGuidePage extends IAdminSection {
        readonly hours: JMSLib.App.IMultiValueFromList<number>;

        readonly sources: JMSLib.App.IMultiValueFromList<string>;

        readonly ukTv: JMSLib.App.IValidatedFlag;

        readonly device: JMSLib.App.IValidateStringFromList;

        readonly remove: JMSLib.App.ICommand;
    }

    export class GuideSection extends AdminSection implements IAdminGuidePage {

        readonly hours = new JMSLib.App.SelectFromList<number>({}, "hours", null, "Uhrzeiten", AdminPage.hoursOfDay);

        readonly sources = new JMSLib.App.SelectFromList<string>({ list: [] }, "list", () => this.refreshUi(), null, []);

        readonly ukTv = new JMSLib.App.EditFlag({}, "includeUK", null, "Sendungsvorschau englischer Sender (FreeSat UK) abrufen");

        readonly remove = new JMSLib.App.Command(() => this.removeSources(), "Entfernen", () => this.sources.value.length > 0);

        readonly device = new JMSLib.App.EditStringFromList({}, "value", () => this.onDeviceChanged(), "Quellen des Gerätes", true, []);

        reset(): void {
            VCRServer.getGuideSettings().then(settings => this.setSettings(settings));
        }

        private setSettings(settings: VCRServer.GuideSettingsContract): void {
            this.hours.data = settings;
            this.ukTv.data = settings;

            this.sources.setValues(settings.sources.map(s => <JMSLib.App.IUiValue<string>>{ value: s, display: s }));
            this.sources.value = [];

            VCRServer.ProfileCache.getAllProfiles().then(profiles => this.setProfiles(profiles));
        }

        private setProfiles(profiles: VCRServer.ProfileInfoContract[]) {
            this.device.allowedValues = profiles.map(p => <JMSLib.App.IUiValue<string>>{ value: p.name, display: p.name });

            if (this.device.allowedValues.length > 0)
                this.device.value = this.device.allowedValues[0].value;

            this.page.application.isBusy = false;

            this.refreshUi();
        }

        private onDeviceChanged(): void {
            if (this.page.application.isBusy)
                return;
        }

        private removeSources(): void {
            var settings = <VCRServer.GuideSettingsContract>this.hours.data;

            this.sources.removeSelected();

            settings.sources = this.sources.allValues;
        }
    }
}