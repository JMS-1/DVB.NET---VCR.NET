/// <reference path="../admin.ts" />
/// <reference path="../../../lib/dateTimeFormatter.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminGuidePage extends IAdminSection {
        readonly hours: JMSLib.App.IMultiValueFromList<number>;
    }

    export class GuideSection extends AdminSection implements IAdminGuidePage {

        static readonly _hours = (() => {
            var hours: JMSLib.App.IUiValue<number>[] = [];

            for (var i = 0; i < 24; i++)
                hours.push({ value: i, display: JMSLib.App.DateFormatter.formatNumber(i) });

            return hours;
        })();

        readonly hours = new JMSLib.App.SelectFromList<number>({}, "hours", null, "Uhrzeiten", GuideSection._hours);

        reset(): void {
            VCRServer.getGuideSettings().then(settings => this.setSettings(settings));
        }

        private setSettings(settings: VCRServer.GuideSettingsContract): void {
            this.hours.data = settings;

            this.page.application.setBusy(false);
        }
    }
}