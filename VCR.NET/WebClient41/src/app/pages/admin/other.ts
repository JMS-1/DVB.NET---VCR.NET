/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminOtherPage extends IAdminSection {
        readonly update: JMSLib.App.ICommand;
    }

    export class OtherSection extends AdminSection implements IAdminOtherPage {

        readonly update = new JMSLib.App.Command(() => this.save(), "Ändern und eventuell neu Starten");

        reset(): void {
            this.update.message = ``;

            VCRServer.getOtherSettings().then(settings => this.setSettings(settings));
        }

        private _other: VCRServer.OtherSettingsContract;

        private setSettings(settings: VCRServer.OtherSettingsContract): void {
            this._other = settings;

            this.page.application.isBusy = false;
        }

        private save(): JMSLib.App.IHttpPromise<void> {
            var settings = <VCRServer.OtherSettingsContract>this._other;

            return this.page.update(VCRServer.setOtherSettings(settings), this.update);
        }
    }
}