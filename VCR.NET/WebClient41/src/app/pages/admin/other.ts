/// <reference path="../admin.ts" />
/// <reference path="../../../lib/edit/list.ts" />

namespace VCRNETClient.App.Admin {

    export enum HibernationMode {
        disabled,

        standBy,

        hibernate
    }

    export interface IAdminOtherPage extends IAdminSection {
        readonly port: JMSLib.App.IValidatedNumber;

        readonly ssl: JMSLib.App.IValidatedFlag;

        readonly securePort: JMSLib.App.IValidatedNumber;

        readonly basicAuth: JMSLib.App.IValidatedFlag;

        readonly preSleep: JMSLib.App.IValidatedNumber;

        readonly minSleep: JMSLib.App.IValidatedNumber;

        readonly ignoreMinSleep: JMSLib.App.IValidatedFlag;

        readonly logKeep: JMSLib.App.IValidatedNumber;

        readonly jobKeep: JMSLib.App.IValidatedNumber;

        readonly noH264PCR: JMSLib.App.IValidatedFlag;

        readonly noMPEG2PCR: JMSLib.App.IValidatedFlag;

        readonly hibernation: JMSLib.App.IValueFromList<HibernationMode>;

        readonly logging: JMSLib.App.IValueFromList<string>;
    }

    export class OtherSection extends AdminSection<VCRServer.OtherSettingsContract> implements IAdminOtherPage {

        private static readonly _logging = [
            JMSLib.App.uiValue("Errors", "Nur Fehler"),
            JMSLib.App.uiValue("Security", "Nur Sicherheitsprobleme"),
            JMSLib.App.uiValue("Schedules", "Aufzeichnungen"),
            JMSLib.App.uiValue("Full", "Vollständig"),
        ];

        private static readonly _hibernation = [
            JMSLib.App.uiValue(HibernationMode.disabled, "Nicht verwenden"),
            JMSLib.App.uiValue(HibernationMode.standBy, "StandBy / Suspend (S3)"),
            JMSLib.App.uiValue(HibernationMode.hibernate, "Hibernate (S4)"),
        ];

        readonly port = new JMSLib.App.EditNumber({}, "webPort", () => this.refreshUi(), "TCP/IP Port für den Web Server", true, 1, 65635);

        readonly ssl = new JMSLib.App.EditFlag({}, "ssl", null, "Sichere Verbindung zusätzlich anbieten");

        readonly securePort = new JMSLib.App.EditNumber({}, "sslPort", () => this.refreshUi(), "TCP/IP Port für den sicheren Zugang", true, 1, 65635);

        readonly basicAuth = new JMSLib.App.EditFlag({}, "basicAuth", null, "Benutzererkennung über Basic (RFC 2617) zusätzlich erlauben (nicht empfohlen)");

        readonly preSleep = new JMSLib.App.EditNumber({}, "hibernationDelay", () => this.refreshUi(), "Vorlaufzeit für das Aufwachen aus dem Schlafzustand in Sekunden", true, 0, 600);

        readonly minSleep = new JMSLib.App.EditNumber({}, "forcedHibernationDelay", () => this.refreshUi(), "Minimale Pause nach einem erzwungenen Schlafzustand in Minuten", true, 5, 60);

        readonly ignoreMinSleep = new JMSLib.App.EditFlag({}, "suppressHibernationDelay", null, "Pause für erzwungenen Schlafzustand ignorieren");

        readonly logKeep = new JMSLib.App.EditNumber({}, "protocol", () => this.refreshUi(), "Aufbewahrungsdauer für Protokolle in Wochen", true, 1, 13);

        readonly jobKeep = new JMSLib.App.EditNumber({}, "archive", () => this.refreshUi(), "Aufbewahrungsdauer von archivierten Aufzeichnungen in Wochen", true, 1, 13);

        readonly noH264PCR = new JMSLib.App.EditFlag({}, "noH264PCR", null, "Systemzeit (PCR) in Aufzeichnungsdateien nicht aus einem H.264 Bildsignal ableiten");

        readonly noMPEG2PCR = new JMSLib.App.EditFlag({}, "noMPEG2PCR", null, "Systemzeit (PCR) in Aufzeichnungsdateien nicht aus einem MPEG2 Bildsignal ableiten");

        readonly hibernation = new JMSLib.App.EditFromList<HibernationMode>({ value: null }, "value", null, "Art des von VCR.NET ausgelösten Schlafzustands", false, OtherSection._hibernation);

        readonly logging = new JMSLib.App.EditFromList<string>({}, "logging", null, "Umfang der Protokollierung in das Windows Ereignisprotokoll", false, OtherSection._logging);

        reset(): void {
            this.update.message = ``;

            VCRServer.getOtherSettings().then(settings => this.setSettings(settings));
        }

        private setSettings(settings: VCRServer.OtherSettingsContract): void {
            this.ignoreMinSleep.data = settings;
            this.noMPEG2PCR.data = settings;
            this.securePort.data = settings;
            this.basicAuth.data = settings;
            this.noH264PCR.data = settings;
            this.minSleep.data = settings;
            this.preSleep.data = settings;
            this.jobKeep.data = settings;
            this.logKeep.data = settings;
            this.logging.data = settings;
            this.port.data = settings;
            this.ssl.data = settings;

            if (settings.mayHibernate)
                if (settings.useStandBy)
                    this.hibernation.value = HibernationMode.standBy;
                else
                    this.hibernation.value = HibernationMode.hibernate;
            else
                this.hibernation.value = HibernationMode.disabled;

            this.page.application.isBusy = false;
        }

        protected readonly saveCaption = "Ändern und eventuell neu Starten";

        protected get canSave(): boolean {
            if (this.port.message !== ``)
                return false;
            if (this.securePort.message !== ``)
                return false;
            if (this.hibernation.message !== ``)
                return false;
            if (this.preSleep.message !== ``)
                return false;
            if (this.minSleep.message !== ``)
                return false;
            if (this.logKeep.message !== ``)
                return false;
            if (this.jobKeep.message !== ``)
                return false;
            if (this.logging.message !== ``)
                return false;

            return true;
        }

        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            var settings: VCRServer.OtherSettingsContract = this.port.data;

            settings.mayHibernate = (this.hibernation.value !== HibernationMode.disabled);
            settings.useStandBy = (this.hibernation.value === HibernationMode.standBy);

            return VCRServer.setOtherSettings(settings);
        }
    }
}