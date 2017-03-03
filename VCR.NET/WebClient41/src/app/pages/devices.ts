/// <reference path="page.ts" />

namespace VCRNETClient.App {

    // Schnittstelle zur Anzeige der aktuellen Aktivitäten.
    export interface IDevicesPage extends IPage {
        // Alle aktuellen Aktivitäten.
        readonly infos: Devices.IDeviceInfo[];
    }

    // Präsentationsmodell zur Anzeige und Manipulation der aktuellen Aktivitäten.
    export class DevicesPage extends Page implements IDevicesPage {

        // Alle Aktivitäten.
        infos: Devices.Info[] = [];

        // Erstellt ein neues Präsentationsmodell.
        constructor(application: Application) {
            super(`current`, application);

            // Der Anwender kann die Ansicht aktualisieren.
            this.navigation.refresh = true;
        }

        // Beginnt mit der Anzeige der Ansicht.
        reset(sections: string[]): void {
            // Zurücksetzen
            this._refreshing = false;
            
            // Liste anfordern.
            this.reload();
        }

        // Gesetzt während sich das Präsentationsmodell aktualisiert.
        private _refreshing = false;

        // Fordert die Aktivitäten vom VCR.NET Recording Service neu an.
        reload(): void {
            VCRServer.getPlanCurrent().then(plan => {
                // Aktionen des Anwenders einmal binden.
                var similiar = this.application.guidePage.findInGuide.bind(this.application.guidePage);
                var refresh = this.toggleDetails.bind(this);
                var reload = this.reload.bind(this);

                // Die aktuellen Aktivitäten umwandeln.
                this.infos = (plan || []).map(info => new Devices.Info(info, this.application.profile.suppressHibernate, refresh, reload, similiar));

                // Anwendung kann nun bedient werden.
                this.application.isBusy = false;

                // Anzeige zur Aktualisierung auffordern.
                this.refreshUi();
            });
        }

        // Schaltet die Detailanzeige einer Aktivität um.
        private toggleDetails(info: Devices.Info, guide: boolean): void {
            // Das machen wir gerade schon.
            if (this._refreshing)
                return;

            // Wir müssen hier Rekursionen vermeiden.
            this._refreshing = true;

            // Aktuellen Stand auslesen.
            var flag = guide ? info.showGuide : info.showControl;
            var state = flag.value;

            // Alle anderen Detailansichten schliessen.
            this.infos.forEach(i => i.showControl.value = i.showGuide.value = false);

            // Neuen Stand übernehmen.
            flag.value = state;

            // Wir können nun wieder normal arbeiten.
            this._refreshing = false;

            // Oberfläche zur Aktualisierung auffordern.
            this.refreshUi();
        }

        // Die Überschreibt für die Ansicht des Präsentationsmodells.
        get title(): string {
            return `Geräteübersicht`;
        }

    }
}