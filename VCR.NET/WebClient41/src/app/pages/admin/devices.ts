﻿/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminDevicesPage extends IAdminSection {
        readonly defaultDevice: JMSLib.App.IValidateStringFromList;

        readonly devices: IDevice[];
    }

    export class DevicesSection extends AdminSection implements IAdminDevicesPage {

        readonly defaultDevice = new JMSLib.App.EditStringFromList({}, "defaultProfile", () => this.refreshUi(), "Bevorzugtes Gerät (zum Beispiel für neue Aufzeichnungen)", true, []);

        devices: Device[] = [];

        reset(): void {
            VCRServer.getProfileSettings().then(settings => this.setProfiles(settings));
        }

        private setProfiles(settings: VCRServer.ProfileSettingsContract): void {
            this.devices = settings.profiles.map(p => new Device(p, () => this.refreshUi()));

            this.defaultDevice.allowedValues = settings.profiles.map(p => <JMSLib.App.IUiValue<string>>{ value: p.name, display: p.name });
            this.defaultDevice.value = settings.defaultProfile;

            this.page.application.setBusy(false);
        }
    }
}