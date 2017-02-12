/// <reference path="section.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminGuidePage extends ISection {
        readonly isActive: JMSLib.App.IEditFlag;

        readonly hours: JMSLib.App.IMultiValueFromList<number>;

        readonly sources: JMSLib.App.IMultiValueFromList<string>;

        readonly ukTv: JMSLib.App.IEditFlag;

        readonly device: JMSLib.App.IValueFromList<string>;

        readonly source: IChannelSelector;

        readonly remove: JMSLib.App.ICommand;

        readonly add: JMSLib.App.ICommand;

        readonly duration: JMSLib.App.IEditNumber;

        readonly delay: JMSLib.App.IEditNumber;

        readonly latency: JMSLib.App.IEditNumber;
    }

    export class GuideSection extends Section<VCRServer.GuideSettingsContract> implements IAdminGuidePage {

        readonly isActive = new JMSLib.App.EditFlag({}, "value", "Aktualisierung aktivieren", () => this.refreshUi());

        readonly hours = new JMSLib.App.SelectFromList<number>({}, "hours", "Uhrzeiten", null, AdminPage.hoursOfDay);

        readonly sources = new JMSLib.App.SelectFromList<string>({ list: [] }, "list", null, () => this.refreshUi(), []);

        readonly ukTv = new JMSLib.App.EditFlag({}, "includeUK", "Sendungsvorschau englischer Sender (FreeSat UK) abrufen", null);

        readonly remove = new JMSLib.App.Command(() => this.removeSources(), "Entfernen", () => this.sources.value.length > 0);

        readonly device = new JMSLib.App.EditFromList<string>({}, "value", "Quellen des Gerätes", () => this.onDeviceChanged(), true, []);

        readonly source: ChannelEditor;

        readonly add = new JMSLib.App.Command(() => this.addSource(), "Hinzufügen", () => this.source.value && (this.sources.allValues.indexOf(this.source.value) < 0));

        readonly duration = new JMSLib.App.EditNumber({}, "duration", "Maximale Laufzeit einer Aktualisierung in Minuten", () => this.refreshUi(), true, 5, 55);

        readonly delay = new JMSLib.App.EditNumber({}, "minDelay", "Wartezeit zwischen zwei Aktualisierungen in Stunden (optional)", () => this.refreshUi(), false, 1, 23);

        readonly latency = new JMSLib.App.EditNumber({}, "joinHours", "Latenzzeit für vorgezogene Aktualisierungen in Stunden (optional)", () => this.refreshUi(), false, 1, 23);

        constructor(page: AdminPage) {
            super(page);

            this.source = new ChannelEditor({}, "value", this.page.application.profile.recentSources || [], () => this.refreshUi());
        }

        protected loadAsync(): void {
            this.add.reset();
            this.remove.reset();

            VCRServer.getGuideSettings().then(settings => this.initialize(settings));
        }

        private initialize(settings: VCRServer.GuideSettingsContract): void {
            this.source.setSources([], true);

            this.duration.data = settings;
            this.latency.data = settings;
            this.hours.data = settings;
            this.delay.data = settings;
            this.ukTv.data = settings;

            this.sources.setValues(settings.sources.map(s => JMSLib.App.uiValue(s)));
            this.sources.value = [];

            this.duration.validate();
            this.latency.validate();
            this.delay.validate();

            this.isActive.value = (settings.duration > 0);

            VCRServer.ProfileCache.getAllProfiles().then(profiles => this.setProfiles(profiles));
        }

        protected get isValid(): boolean {
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
            this.device.allowedValues = profiles.map(p => JMSLib.App.uiValue(p.name));

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

            this.sources.addValue(JMSLib.App.uiValue(source));

            this.source.value = ``;

            var settings = <VCRServer.GuideSettingsContract>this.hours.data;

            settings.sources = this.sources.allValues;
        }

        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            var settings = <VCRServer.GuideSettingsContract>this.hours.data;

            if (!this.isActive.value)
                settings.duration = 0;

            return VCRServer.setGuideSettings(settings);
        }
    }
}