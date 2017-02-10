/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminScanPage extends IAdminSection {
        readonly mode: JMSLib.App.IValueFromList<string>;

        readonly showConfiguration: boolean;

        readonly configureAutomatic: boolean;

        readonly duration: JMSLib.App.IValidatedNumber;

        readonly hours: JMSLib.App.IMultiValueFromList<number>;

        readonly merge: JMSLib.App.IValidatedFlag;

        readonly gapDays: JMSLib.App.IValidatedNumber;

        readonly latency: JMSLib.App.IValidatedNumber;

        readonly update: JMSLib.App.ICommand;
    }

    export class ScanSection extends AdminSection implements IAdminScanPage {
        private static readonly  _scanDisabled = "Aktualisierung deaktivieren";

        private static readonly  _scanManual = "Manuell aktualisieren";

        private static readonly  _scanAutomatic = "Aktualisieren nach Zeitplan";

        private static readonly _scanModes: JMSLib.App.IUiValue<string>[] = [
            { value: ScanSection._scanDisabled, display: ScanSection._scanDisabled },
            { value: ScanSection._scanManual, display: ScanSection._scanManual },
            { value: ScanSection._scanAutomatic, display: ScanSection._scanAutomatic },
        ];

        readonly mode = new JMSLib.App.EditStringFromList({}, "value", () => this.refreshUi(), null, true, ScanSection._scanModes);

        readonly hours = new JMSLib.App.SelectFromList<number>({}, "hours", null, "Uhrzeiten", AdminPage.hoursOfDay);

        readonly duration = new JMSLib.App.EditNumber({}, "duration", () => this.refreshUi(), "Maximale Laufzeit für einen Sendersuchlauf in Minuten", true, 5, 55);

        readonly merge = new JMSLib.App.EditFlag({}, "merge", null, "Senderliste nach dem Suchlauf mit der vorherigen zusammenführen (empfohlen)");

        readonly gapDays = new JMSLib.App.EditNumber({}, "interval", () => this.refreshUi(), "Minimale Anzahl von Tagen zwischen zwei Suchläufen", true, 1, 28);

        readonly latency = new JMSLib.App.EditNumber({}, "joinDays", () => this.refreshUi(), "Latenzzeit für vorgezogene Aktualisierungen in Tagen (optional)", false, 1, 14);

        readonly update = new JMSLib.App.Command(() => this.save(), "Ändern", () => this.isValid);

        get showConfiguration(): boolean {
            return this.mode.value !== ScanSection._scanDisabled;
        }

        get configureAutomatic(): boolean {
            return this.mode.value === ScanSection._scanAutomatic;
        }

        reset(): void {
            this.update.message = ``;

            VCRServer.getSourceScanSettings().then(settings => this.setSettings(settings));
        }

        private setSettings(settings: VCRServer.SourceScanSettingsContract): void {
            this.duration.data = settings;
            this.gapDays.data = settings;
            this.latency.data = settings;
            this.hours.data = settings;
            this.merge.data = settings;

            if (settings.interval === null)
                this.mode.value = ScanSection._scanDisabled;
            else if (settings.interval < 0) {
                settings.interval = null;

                this.mode.value = ScanSection._scanManual;
            }
            else
                this.mode.value = ScanSection._scanAutomatic;

            this.duration.validate();
            this.gapDays.validate();
            this.latency.validate();

            this.page.application.isBusy = false;
        }

        private get isValid(): boolean {
            if (this.mode.value === ScanSection._scanDisabled)
                return true;

            if (this.duration.message !== ``)
                return false;

            if (this.mode.value === ScanSection._scanManual)
                return true;

            if (this.gapDays.message !== ``)
                return false;

            if (this.latency.message !== ``)
                return false;

            return true;
        }

        private save(): JMSLib.App.IHttpPromise<void> {
            var settings = <VCRServer.SourceScanSettingsContract>this.hours.data;

            if (this.mode.value === ScanSection._scanDisabled)
                settings.interval = 0;
            else if (this.mode.value === ScanSection._scanManual)
                settings.interval = -1;

            return this.page.update(VCRServer.setSourceScanSettings(settings), this.update);
        }
    }
}