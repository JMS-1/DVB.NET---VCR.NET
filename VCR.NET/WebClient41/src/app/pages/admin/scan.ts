/// <reference path="section.ts" />

namespace VCRNETClient.App.Admin {

    // Die Art der Aktualisierung der Quellenlisten.
    export enum ScanConfigMode {
        // Eine Aktualisierung ist nicht möglich.
        disabled,

        // Die Aktualisierung wird manuell gestartet.
        manual,

        // Die Aktualisierung wird nach einem Zeitplan durchgeführt.
        automatic
    }

    // Schnittstelle zur Konfiguration des Sendersuchlaufs.
    export interface IAdminScanPage extends ISection {
        // Die Art der Aktualisierung.
        readonly mode: JMSLib.App.IValueFromList<ScanConfigMode>;

        // Gesetzt, wenn die Konfiguration überhaupt angezeigt werden soll.
        readonly showConfiguration: boolean;

        // Gesetzt, wenn die Einstellungen der automatischen Aktualisierung angezeigt werden sollen.
        readonly configureAutomatic: boolean;

        // Die maximale Dauer eines Suchlaufs (in Minuten).
        readonly duration: JMSLib.App.INumber;

        // Die Stunden, an denen eine Aktualisierung ausgeführt werden soll.
        readonly hours: JMSLib.App.IMultiValueFromList<number>;

        // Gesetzt, wenn das Ergebnis der Aktualisierung mit der aktuellen Liste der Quellen zusammengeführt werden soll.
        readonly merge: JMSLib.App.IFlag;

        // Die minimale zeit zwischen zwei automatischen Aktualisierungen (in Tagen).
        readonly gapDays: JMSLib.App.INumber;

        // Die Zeit für eine vorgezogene Aktualisierung (in Tagen).
        readonly latency: JMSLib.App.INumber;
    }

    // Präsentationsmodell zur Pflege der Konfiguration des Sendersuchlaufs.
    export class ScanSection extends Section implements IAdminScanPage {

        // Die Anzeigewerte für die einzelnen Arten der Aktualisierung.
        private static readonly _scanModes = [
            JMSLib.App.uiValue(ScanConfigMode.disabled, "Aktualisierung deaktivieren"),
            JMSLib.App.uiValue(ScanConfigMode.manual, "Manuell aktualisieren"),
            JMSLib.App.uiValue(ScanConfigMode.automatic, "Aktualisieren nach Zeitplan"),
        ];

        // Die Art der Aktualisierung.
        readonly mode = new JMSLib.App.SelectSingleFromList({}, "value", null, () => this.refreshUi(), ScanSection._scanModes);

        // Die Stunden, an denen eine Aktualisierung ausgeführt werden soll.
        readonly hours = new JMSLib.App.SelectMultipleFromList({}, "hours", "Uhrzeiten", null, AdminPage.hoursOfDay);

        // Die maximale Dauer eines Suchlaufs (in Minuten).
        readonly duration = new JMSLib.App.Number({}, "duration", "Maximale Laufzeit für einen Sendersuchlauf in Minuten", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(5)
            .addMaxValidator(55);

        // Gesetzt, wenn das Ergebnis der Aktualisierung mit der aktuellen Liste der Quellen zusammengeführt werden soll.
        readonly merge = new JMSLib.App.Flag({}, "merge", "Senderliste nach dem Suchlauf mit der vorherigen zusammenführen (empfohlen)");

        // Die minimale zeit zwischen zwei automatischen Aktualisierungen (in Tagen).
        readonly gapDays = new JMSLib.App.Number({}, "interval", "Minimale Anzahl von Tagen zwischen zwei Suchläufen", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(1)
            .addMaxValidator(28);

        // Die Zeit für eine vorgezogene Aktualisierung (in Tagen).
        readonly latency = new JMSLib.App.Number({}, "joinDays", "Latenzzeit für vorgezogene Aktualisierungen in Tagen (optional)", () => this.update.refreshUi())
            .addMinValidator(1)
            .addMaxValidator(14);

        // Gesetzt, wenn die Konfiguration überhaupt angezeigt werden soll.
        get showConfiguration(): boolean {
            return this.mode.value !== ScanConfigMode.disabled;
        }

        // Gesetzt, wenn die Einstellungen der automatischen Aktualisierung angezeigt werden sollen.
        get configureAutomatic(): boolean {
            return this.mode.value === ScanConfigMode.automatic;
        }

        // Forder die aktuelle Konfiguration vom VCR.NET Recordings Service an.
        protected loadAsync(): void {
            VCRServer.getSourceScanSettings().then(settings => {
                // Präsentationsmodell mit den Daten verbinden.
                this.duration.data = settings;
                this.gapDays.data = settings;
                this.latency.data = settings;
                this.hours.data = settings;
                this.merge.data = settings;

                // Die Art erfordert eine Sonderbehandlung.
                if (settings.interval === null)
                    this.mode.value = ScanConfigMode.disabled;
                else if (settings.interval < 0) {
                    settings.interval = null;

                    this.mode.value = ScanConfigMode.manual;
                }
                else
                    this.mode.value = ScanConfigMode.automatic;

                // Anwendung zur Bedienung freischalten.
                this.page.application.isBusy = false;

                // Anzeige zur Aktualisierung auffordern.
                this.refreshUi();
            });
        }

        // Prüft alle Einstellungen auf Konsistenz.
        protected get isValid(): boolean {
            // Nicht notwendig wenn alles deaktiviert ist.
            if (!this.showConfiguration)
                return true;

            // Zumindest die Grundeinstellungen für die manuelle Aktualisierung prüfen.
            if (this.duration.message)
                return false;

            // Zusätzlich eventuell auch noch die Einstellungen der automatischen Aktualisierung.
            if (!this.configureAutomatic)
                return true;
            if (this.gapDays.message)
                return false;
            if (this.latency.message)
                return false;

            // Speicherung ist möglich.
            return true;
        }

        // Fordert den VCR.NET Recording Service zur Aktualisierung der Konfiguration an.
        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            // Die Art wird in die Konfigurationsdaten zurückgespiegelt.
            var settings = <VCRServer.SourceScanSettingsContract>this.hours.data;

            if (!this.showConfiguration)
                settings.interval = 0;
            else if (!this.configureAutomatic)
                settings.interval = -1;

            // Änderung der Konfiguration asynchron starten.
            return VCRServer.setSourceScanSettings(settings);
        }
    }
}