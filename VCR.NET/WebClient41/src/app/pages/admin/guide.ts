/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminGuidePage extends IAdminSection {
        readonly isActive: JMSLib.App.IValidatedFlag;

        readonly hours: JMSLib.App.IMultiValueFromList<number>;

        readonly sources: JMSLib.App.IMultiValueFromList<string>;

        readonly ukTv: JMSLib.App.IValidatedFlag;

        readonly device: JMSLib.App.IValueFromList<string>;

        readonly source: IChannelSelector;

        readonly remove: JMSLib.App.ICommand;

        readonly add: JMSLib.App.ICommand;

        readonly duration: JMSLib.App.IValidatedNumber;

        readonly delay: JMSLib.App.IValidatedNumber;

        readonly latency: JMSLib.App.IValidatedNumber;

        readonly update: JMSLib.App.ICommand;
    }

    export class GuideSection extends AdminSection implements IAdminGuidePage {

        readonly isActive = new JMSLib.App.EditFlag({}, "value", () => this.refreshUi(), "Aktualisierung aktivieren");

        readonly hours = new JMSLib.App.SelectFromList<number>({}, "hours", null, "Uhrzeiten", AdminPage.hoursOfDay);

        readonly sources = new JMSLib.App.SelectFromList<string>({ list: [] }, "list", () => this.refreshUi(), null, []);

        readonly ukTv = new JMSLib.App.EditFlag({}, "includeUK", null, "Sendungsvorschau englischer Sender (FreeSat UK) abrufen");

        readonly remove = new JMSLib.App.Command(() => this.removeSources(), "Entfernen", () => this.sources.value.length > 0);

        readonly device = new JMSLib.App.EditFromList<string>({}, "value", () => this.onDeviceChanged(), "Quellen des Gerätes", true, []);

        readonly source: ChannelEditor;

        readonly add = new JMSLib.App.Command(() => this.addSource(), "Hinzufügen", () => this.source.value && (this.sources.allValues.indexOf(this.source.value) < 0));

        readonly duration = new JMSLib.App.EditNumber({}, "duration", () => this.refreshUi(), "Maximale Laufzeit einer Aktualisierung in Minuten", true, 5, 55);

        readonly delay = new JMSLib.App.EditNumber({}, "minDelay", () => this.refreshUi(), "Wartezeit zwischen zwei Aktualisierungen in Stunden (optional)", false, 1, 23);

        readonly latency = new JMSLib.App.EditNumber({}, "joinHours", () => this.refreshUi(), "Latenzzeit für vorgezogene Aktualisierungen in Stunden (optional)", false, 1, 23);

        readonly update = new JMSLib.App.Command(() => this.save(), "Ändern", () => this.isValid);

        constructor(page: AdminPage) {
            super(page);

            this.source = new ChannelEditor({}, "value", this.page.application.profile.recentSources || [], () => this.refreshUi());
        }

        reset(): void {
            this.update.message = ``;
            this.source.setSources([], true);

            VCRServer.getGuideSettings().then(settings => this.setSettings(settings));
        }

        private setSettings(settings: VCRServer.GuideSettingsContract): void {
            this.duration.data = settings;
            this.latency.data = settings;
            this.hours.data = settings;
            this.delay.data = settings;
            this.ukTv.data = settings;

            this.sources.setValues(settings.sources.map(s => <JMSLib.App.IUiValue<string>>{ value: s, display: s }));
            this.sources.value = [];

            this.duration.validate();
            this.latency.validate();
            this.delay.validate();

            this.isActive.value = (settings.duration > 0);

            VCRServer.ProfileCache.getAllProfiles().then(profiles => this.setProfiles(profiles));
        }

        private get isValid(): boolean {
            if (!this.isActive.value)
                return true;

            if (this.duration.message !== ``)
                return false;

            if (this.latency.message !== ``)
                return false;

            if (this.delay.message !== ``)
                return false;

            return true;
        }

        private setProfiles(profiles: VCRServer.ProfileInfoContract[]) {
            this.device.allowedValues = profiles.map(p => <JMSLib.App.IUiValue<string>>{ value: p.name, display: p.name });

            if (this.device.allowedValues.length > 0)
                this.device.value = this.device.allowedValues[0].value;

            this.loadSources();
        }

        private onDeviceChanged(): void {
            if (this.page.application.isBusy)
                return;

            this.loadSources();
        }

        private loadSources(): void {
            VCRServer.ProfileSourcesCache.getSources(this.device.value).then(sources => {
                this.source.setSources(sources, false);
                this.source.value = ``;

                this.page.application.isBusy = false;

                this.refreshUi();
            });
        }

        private removeSources(): void {
            this.sources.removeSelected();

            var settings = <VCRServer.GuideSettingsContract>this.hours.data;

            settings.sources = this.sources.allValues;
        }

        private addSource(): void {
            var source = this.source.value;

            this.sources.addValue({ value: source, display: source });

            this.source.value = ``;

            var settings = <VCRServer.GuideSettingsContract>this.hours.data;

            settings.sources = this.sources.allValues;
        }

        private save(): JMSLib.App.IHttpPromise<void> {
            var settings = <VCRServer.GuideSettingsContract>this.hours.data;

            if (!this.isActive.value)
                settings.duration = 0;

            return this.page.update(VCRServer.setGuideSettings(settings), this.update);
        }
    }
}