/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    // Schnittstelle zur Konfiguration der Benutzergruppen.
    export interface IAdminSecurityPage extends IAdminSection {
        // Die Gruppe der normalen Benutzer.
        readonly userGroups: JMSLib.App.IValueFromList<string>;

        // Die Gruppe der Administratoren.
        readonly adminGroups: JMSLib.App.IValueFromList<string>;

        // Befehl zur Aktualisierung der Konfiguration.
        readonly update: JMSLib.App.ICommand;
    }

    // Instanz zur Pflege der Konfiguration der Benutzergruppen.
    export class SecuritySection extends AdminSection implements IAdminSecurityPage {

        // Alle bekannten Windows Kontogruppen.
        private static _windowsGroups: JMSLib.App.IHttpPromise<JMSLib.App.IUiValue<string>[]>;

        // Die Gruppe der normalen Benutzer mit Auswahl.
        readonly userGroups = new JMSLib.App.EditFromList<string>({}, `users`, null, `Benutzer`, false, []);

        // Die Gruppe der Administratoren mit Auswahl.
        readonly adminGroups = new JMSLib.App.EditFromList<string>({}, `admins`, null, `Administratoren`, false, []);

        // Befehl zum Aktualisieren der Konfiguration.
        readonly update = new JMSLib.App.Command(() => this.save(), `Ändern`);

        // Initialisiert die Pflegeumgebung.
        reset(): void {
            // Fehleranzeige zurücksetzen.
            this.update.message = ``;

            // Windows Kontogruppen einmalig anfordern.
            if (!SecuritySection._windowsGroups)
                SecuritySection._windowsGroups = VCRServer.getWindowsGroups().then(names => {
                    // Immer die Leerauswahl ergänzen - damit werden automatisch alle Benutzer erfasst.
                    var groups = [<JMSLib.App.IUiValue<string>>{ display: `(Alle Benutzer)`, value: `` }];

                    // Auswahlliste aufsetzen und melden.
                    groups.push(...names.map(name => <JMSLib.App.IUiValue<string>>{ display: name, value: name }));

                    return groups;
                });

            // Windows Kontogruppen direkt oder verzögert laden.
            SecuritySection._windowsGroups.then(groups => this.setWindowsGroups(groups));
        }

        // Benutzergruppen speichern.
        private save(): JMSLib.App.IHttpPromise<void> {
            var settings: VCRServer.SecuritySettingsContract = this.userGroups.data;

            return this.page.update(VCRServer.setSecuritySettings(settings), this.update);
        }

        // Windows Kontogruppen in die Auswahllisten übernehmen.
        private setWindowsGroups(groups: JMSLib.App.IUiValue<string>[]): void {
            this.userGroups.allowedValues = groups;
            this.adminGroups.allowedValues = groups;

            // Aktuelle Benutzergruppen anfordern.
            VCRServer.getSecuritySettings().then(settings => this.setSecurity(settings));
        }

        // Aktuelle Benutzergruppen in den Auswahhlisten vorwählen.
        private setSecurity(security: VCRServer.SecuritySettingsContract): void {
            this.userGroups.data = this.adminGroups.data = security;

            // Bedienung durch den Anwender freischalten.
            this.page.application.isBusy = false;
        }
    }
}