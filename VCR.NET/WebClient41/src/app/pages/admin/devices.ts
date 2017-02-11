/// <reference path="section.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminDevicesPage extends ISection {
        readonly defaultDevice: JMSLib.App.IValueFromList<string>;

        readonly devices: IDevice[];
    }

    export class DevicesSection extends Section<VCRServer.ProfileSettingsContract> implements IAdminDevicesPage {

        static readonly sectionName = "Geräte";

        readonly defaultDevice = new JMSLib.App.EditFromList<string>({}, "defaultProfile", () => this.refreshUi(), "Bevorzugtes Gerät (zum Beispiel für neue Aufzeichnungen)", true, []);

        devices: Device[] = [];

        reset(): void {
            this.update.message = ``;

            VCRServer.getProfileSettings().then(settings => this.setProfiles(settings));
        }

        private setProfiles(settings: VCRServer.ProfileSettingsContract): void {
            this.devices = settings.profiles.map(p => new Device(p, () => this.refreshUi()));

            this.defaultDevice.allowedValues = settings.profiles.map(p => JMSLib.App.uiValue(p.name));
            this.defaultDevice.data = settings;

            this.refreshUi();

            this.page.application.isBusy = false;
        }

        refreshUi(): void {
            this.devices.forEach(d => d.validate(this.defaultDevice.value));

            this.defaultDevice.message = this.devices.some(d => d.active.message !== ``) ? `Dieses Gerät ist nicht für Aufzeichnungen vorgesehen` : ``;

            super.refreshUi();
        }

        protected readonly saveCaption = "Ändern und neu Starten";

        protected get canSave(): boolean {
            return this.devices.every(d => d.isValid);
        }

        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            return VCRServer.setProfileSettings(this.defaultDevice.data);
        }
    }
}