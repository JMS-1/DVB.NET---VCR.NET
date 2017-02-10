/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminOtherPage extends IAdminSection {
    }

    export class OtherSection extends AdminSection<VCRServer.OtherSettingsContract> implements IAdminOtherPage {

        reset(): void {
            this.update.message = ``;

            VCRServer.getOtherSettings().then(settings => this.setSettings(settings));
        }

        private _other: VCRServer.OtherSettingsContract;

        private setSettings(settings: VCRServer.OtherSettingsContract): void {
            this._other = settings;

            this.page.application.isBusy = false;
        }

        protected readonly saveCaption = "Ändern und eventuell neu Starten";

        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            return VCRServer.setOtherSettings(this._other);
        }
    }
}