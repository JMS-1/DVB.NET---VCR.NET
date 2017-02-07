/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminDevicesPage extends IAdminSection {
        readonly defaultDevice: JMSLib.App.IValidateStringFromList;

        readonly devices: IDevice[];

        readonly update: JMSLib.App.ICommand;
    }

    export class DevicesSection extends AdminSection implements IAdminDevicesPage {

        readonly defaultDevice = new JMSLib.App.EditStringFromList({}, "defaultProfile", () => this.refreshUi(), "Bevorzugtes Gerät (zum Beispiel für neue Aufzeichnungen)", true, []);

        readonly update = new JMSLib.App.Command(() => this.save(), "Ändern und neu Starten", () => this.isValid)

        devices: Device[] = [];

        reset(): void {
            VCRServer.getProfileSettings().then(settings => this.setProfiles(settings));
        }

        private setProfiles(settings: VCRServer.ProfileSettingsContract): void {
            this.devices = settings.profiles.map(p => new Device(p, () => this.refreshUi()));

            this.defaultDevice.allowedValues = settings.profiles.map(p => <JMSLib.App.IUiValue<string>>{ value: p.name, display: p.name });
            this.defaultDevice.data = settings;

            this.refreshUi();

            this.page.application.isBusy = false;
        }

        refreshUi(): void {
            this.devices.forEach(d => d.validate(this.defaultDevice.value));

            this.defaultDevice.message = this.devices.some(d => d.active.message !== ``) ? `Dieses Gerät ist nicht für Aufzeichnungen vorgesehen` : ``;

            super.refreshUi();
        }

        private get isValid(): boolean {
            return this.devices.every(d => d.isValid);
        }

        private save(): JMSLib.App.IHttpPromise<void> {
            var settings: VCRServer.ProfileSettingsContract = this.defaultDevice.data;

            return this.page.update(VCRServer.setProfileSettings(settings));
        }
    }
}