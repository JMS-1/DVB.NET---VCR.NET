/// <reference path="section.ts" />
/// <reference path="../../../lib/edit/list.ts" />

namespace VCRNETClient.App.Admin {

    export enum ScanConfigMode {
        disabled,

        manual,

        automatic
    }

    export interface IAdminScanPage extends ISection {
        readonly mode: JMSLib.App.IValueFromList<ScanConfigMode>;

        readonly showConfiguration: boolean;

        readonly configureAutomatic: boolean;

        readonly duration: JMSLib.App.INumber;

        readonly hours: JMSLib.App.IMultiValueFromList<number>;

        readonly merge: JMSLib.App.IFlag;

        readonly gapDays: JMSLib.App.INumber;

        readonly latency: JMSLib.App.INumber;
    }

    export class ScanSection extends Section implements IAdminScanPage {

        private static readonly  _scanDisabled = "Aktualisierung deaktivieren";

        private static readonly  _scanManual = "Manuell aktualisieren";

        private static readonly  _scanAutomatic = "Aktualisieren nach Zeitplan";

        private static readonly _scanModes = [
            JMSLib.App.uiValue(ScanConfigMode.disabled, ScanSection._scanDisabled),
            JMSLib.App.uiValue(ScanConfigMode.manual, ScanSection._scanManual),
            JMSLib.App.uiValue(ScanConfigMode.automatic, ScanSection._scanAutomatic),
        ];

        readonly mode = new JMSLib.App.SelectSingleFromList<ScanConfigMode>({}, "value", null, () => this.refreshUi(), true, ScanSection._scanModes);

        readonly hours = new JMSLib.App.SelectMultipleFromList<number>({}, "hours", "Uhrzeiten", null, AdminPage.hoursOfDay);

        readonly duration = new JMSLib.App.Number({}, "duration", "Maximale Laufzeit für einen Sendersuchlauf in Minuten", () => this.refreshUi(), true, 5, 55);

        readonly merge = new JMSLib.App.Flag({}, "merge", "Senderliste nach dem Suchlauf mit der vorherigen zusammenführen (empfohlen)", null);

        readonly gapDays = new JMSLib.App.Number({}, "interval", "Minimale Anzahl von Tagen zwischen zwei Suchläufen", () => this.refreshUi(), true, 1, 28);

        readonly latency = new JMSLib.App.Number({}, "joinDays", "Latenzzeit für vorgezogene Aktualisierungen in Tagen (optional)", () => this.refreshUi(), false, 1, 14);

        get showConfiguration(): boolean {
            return this.mode.value !== ScanConfigMode.disabled;
        }

        get configureAutomatic(): boolean {
            return this.mode.value === ScanConfigMode.automatic;
        }

        protected loadAsync(): void {
            VCRServer.getSourceScanSettings().then(settings => this.initialize(settings));
        }

        private initialize(settings: VCRServer.SourceScanSettingsContract): void {
            this.duration.data = settings;
            this.gapDays.data = settings;
            this.latency.data = settings;
            this.hours.data = settings;
            this.merge.data = settings;

            if (settings.interval === null)
                this.mode.value = ScanConfigMode.disabled;
            else if (settings.interval < 0) {
                settings.interval = null;

                this.mode.value = ScanConfigMode.manual;
            }
            else
                this.mode.value = ScanConfigMode.automatic;

            this.duration.validate();
            this.gapDays.validate();
            this.latency.validate();

            this.page.application.isBusy = false;
        }

        protected get isValid(): boolean {
            if (!this.showConfiguration)
                return true;

            if (this.duration.message !== ``)
                return false;

            if (!this.configureAutomatic)
                return true;

            if (this.gapDays.message !== ``)
                return false;

            if (this.latency.message !== ``)
                return false;

            return true;
        }

        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            var settings = <VCRServer.SourceScanSettingsContract>this.hours.data;

            if (!this.showConfiguration)
                settings.interval = 0;
            else if (!this.configureAutomatic)
                settings.interval = -1;

            return VCRServer.setSourceScanSettings(settings);
        }
    }
}