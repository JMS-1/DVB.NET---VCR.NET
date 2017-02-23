/// <reference path="section.ts" />

namespace VCRNETClient.App.Admin {

    // Schnittstelle zur Konfiguration der Geräteprofile.
    export interface IAdminDevicesPage extends ISection {
        // Erlaubt die Auswahl des bevorzugten Gerätes.
        readonly defaultDevice: JMSLib.App.IValueFromList<string>;

        // Alle bekannten Geräte.
        readonly devices: IDevice[];
    }

    // Präsentationsmodell zur Konfiguration der Geräteprofile.
    export class DevicesSection extends Section implements IAdminDevicesPage {

        // Präsentationsmodell zur Pflege des bevorzugten Gerätes.
        readonly defaultDevice = new JMSLib.App.SelectSingleFromList<string>({}, "defaultProfile", "Bevorzugtes Gerät (zum Beispiel für neue Aufzeichnungen)", () => this.refresh(), true, [], list => this.validateDefaultDevice(list.value));

        // Alle bekannten Gerät.
        devices: Device[] = [];

        // Fordert die Konfiguration der Geräteprofile an.
        protected loadAsync(): void {
            VCRServer.getProfileSettings().then(settings => {
                // Präsentationsmodelle aus den Rohdaten erstellen.
                this.devices = settings.profiles.map(p => new Device(p, () => this.refresh(), () => this.defaultDevice.value));

                // Liste der Geräte ermitteln und in die Auswahl für das bevorzugte Gerät übernehmen.
                this.defaultDevice.allowedValues = settings.profiles.map(p => JMSLib.App.uiValue(p.name));
                this.defaultDevice.data = settings;

                // Initiale Prüfung durchführen.
                this.refresh();

                // Anwendung freischalten.
                this.page.application.isBusy = false;
            });
        }

        // Prüft, ob das bevorzugte Gerät auch verwendet werden darf.
        private validateDefaultDevice(defaultDevice: string): string {
            if (this.devices.filter(d => d.name === defaultDevice).some(d => !d.active.value))
                return `Dieses Gerät ist nicht für Aufzeichnungen vorgesehen`;
        }

        // Aktualisiert die Anzeige.
        refresh(): void {
            // Alle Eingaben gemeinsam prüfen.
            this.devices.forEach(d => d.validate());
            this.defaultDevice.validate();

            // Oberfläche zur Aktualisierung auffordern.
            this.refreshUi();
        }

        // Beschriftung der Schaltfläche zur Aktualisierung der Konfiguration.
        protected readonly saveCaption = "Ändern und neu Starten";

        // Gesetzt, wenn ein Speichern möglich ist.
        protected get isValid(): boolean {
            return this.devices.every(d => d.isValid);
        }

        // Sendet die Konfiguration zur asynchronen Aktualisierung an den VCR.NET Recording Service.
        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            return VCRServer.setProfileSettings(this.defaultDevice.data);
        }
    }
}