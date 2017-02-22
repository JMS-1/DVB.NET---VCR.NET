/// <reference path="section.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminDevicesPage extends ISection {
        readonly defaultDevice: JMSLib.App.IValueFromList<string>;

        readonly devices: IDevice[];
    }

    export class DevicesSection extends Section<VCRServer.ProfileSettingsContract> implements IAdminDevicesPage {

        readonly defaultDevice = new JMSLib.App.SelectSingleFromList<string>({}, "defaultProfile", "Bevorzugtes Gerät (zum Beispiel für neue Aufzeichnungen)", () => this.refreshUi(), true, [], list => this.validateDefaultDevice());

        devices: Device[] = [];

        protected loadAsync(): void {
            VCRServer.getProfileSettings().then(settings => this.initialize(settings));
        }

        private initialize(settings: VCRServer.ProfileSettingsContract): void {
            this.devices = settings.profiles.map(p => new Device(p, () => this.refreshUi()));

            this.defaultDevice.allowedValues = settings.profiles.map(p => JMSLib.App.uiValue(p.name));
            this.defaultDevice.data = settings;

            this.refreshUi();

            this.page.application.isBusy = false;
        }

        private validateDefaultDevice(): string {
            return this.devices.some(d => d.active.message !== ``) ? `Dieses Gerät ist nicht für Aufzeichnungen vorgesehen` : ``;
        }

        refreshUi(): void {
            this.devices.forEach(d => d.validate(this.defaultDevice.value));
            this.defaultDevice.validate();

            super.refreshUi();
        }

        protected readonly saveCaption = "Ändern und neu Starten";

        protected get isValid(): boolean {
            return this.devices.every(d => d.isValid);
        }

        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            return VCRServer.setProfileSettings(this.defaultDevice.data);
        }
    }
}