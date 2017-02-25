/// <reference path="section.ts" />
/// <reference path="../../../lib/edit/list.ts" />

namespace VCRNETClient.App.Admin {

    // Die Art des zu verwendenden Schlafzustands.
    export enum HibernationMode {
        // Kein automatischer Übergang in den Schlafzustand.
        disabled,

        // Für den Schlafzustand Stand-By verwenden.
        standBy,

        // Für den Schlafzustand Hibernate verwenden.
        hibernate
    }

    // Schnittstelle zur Pflege sonstiger Konfigurationswerte.
    export interface IAdminOtherPage extends ISection {
        // Der TCP/IP Port des Web Clients.
        readonly port: JMSLib.App.INumber;

        // Gesetzt, wenn auch eine sichere Verbindung (SSL / HTTPS) unterstützt werden soll.
        readonly ssl: JMSLib.App.IFlag;

        // Der sichere (SSL) TCP/IP Port des Web Clients.
        readonly securePort: JMSLib.App.INumber;

        // Gesetzt, wenn neben der integrierten Windows Sicherheit (NTLM Challenge/Response) auch die Standard Autorisierung (Basic) verwendet werden kann.
        readonly basicAuth: JMSLib.App.IFlag;

        // Die Zeit zum vorzeitigen Aufwachen für eine Aufzeichnung oder Sonderaufgabe (in Sekunden).
        readonly preSleep: JMSLib.App.INumber;

        // Die minimale Verweildauer im Schalfzustand (in Minuten).
        readonly minSleep: JMSLib.App.INumber;

        // Gesetzt um die minimale Verweildauer im Schlafzustand zu unterdrücken.
        readonly ignoreMinSleep: JMSLib.App.IFlag;

        // Die Verweildauer eines Protokolleintrags vor der automatischen Löscung (in Wochen).
        readonly logKeep: JMSLib.App.INumber;

        // Die Verweildauer eines Auftrags im Archiv vor der automatischen Löschung (in Wochen).
        readonly jobKeep: JMSLib.App.INumber;

        // Gesetzt, wenn die Systemzeit einer HDTV Aufzeichnung nicht automatisch ermittelt werden soll.
        readonly noH264PCR: JMSLib.App.IFlag;

        // Gesetzt, wenn die Systemzeit einer SDTV Aufzeichnung nicht automatisch ermittelt werden soll.
        readonly noMPEG2PCR: JMSLib.App.IFlag;

        // Die Art des automatischen Schlafzustands.
        readonly hibernation: JMSLib.App.IValueFromList<HibernationMode>;

        // Die Art der Protokollierung.
        readonly logging: JMSLib.App.IValueFromList<string>;
    }

    // Präsentationsmodell zur Pflege sonstiger Konfigurationswerte.
    export class OtherSection extends Section implements IAdminOtherPage {

        // Die einzelnen Arten der Protokollierung als Auswahlliste für den Anwender.
        private static readonly _logging = [
            JMSLib.App.uiValue("Errors", "Nur Fehler"),
            JMSLib.App.uiValue("Security", "Nur Sicherheitsprobleme"),
            JMSLib.App.uiValue("Schedules", "Aufzeichnungen"),
            JMSLib.App.uiValue("Full", "Vollständig"),
        ];

        // Die einzelnen Arten des Schlafzustands als Auswahlliste für den Anwender.
        private static readonly _hibernation = [
            JMSLib.App.uiValue(HibernationMode.disabled, "Nicht verwenden"),
            JMSLib.App.uiValue(HibernationMode.standBy, "StandBy / Suspend (S3)"),
            JMSLib.App.uiValue(HibernationMode.hibernate, "Hibernate (S4)"),
        ];

        // Der TCP/IP Port des Web Clients.
        readonly port = new JMSLib.App.Number({}, "webPort", "TCP/IP Port für den Web Server", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(1)
            .addMaxValidator(0xffff);

        // Gesetzt, wenn auch eine sichere Verbindung (SSL / HTTPS) unterstützt werden soll.
        readonly ssl = new JMSLib.App.Flag({}, "ssl", "Sichere Verbindung zusätzlich anbieten");

        // Der sichere (SSL) TCP/IP Port des Web Clients.
        readonly securePort = new JMSLib.App.Number({}, "sslPort", "TCP/IP Port für den sicheren Zugang", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(1)
            .addMaxValidator(0xffff);

        // Gesetzt, wenn neben der integrierten Windows Sicherheit (NTLM Challenge/Response) auch die Standard Autorisierung (Basic) verwendet werden kann.
        readonly basicAuth = new JMSLib.App.Flag({}, "basicAuth", "Benutzererkennung über Basic (RFC 2617) zusätzlich erlauben (nicht empfohlen)");

        // Die Zeit zum vorzeitigen Aufwachen für eine Aufzeichnung oder Sonderaufgabe (in Sekunden).
        readonly preSleep = new JMSLib.App.Number({}, "hibernationDelay", "Vorlaufzeit für das Aufwachen aus dem Schlafzustand in Sekunden", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(0)
            .addMaxValidator(600);

        // Die minimale Verweildauer im Schalfzustand (in Minuten).
        readonly minSleep = new JMSLib.App.Number({}, "forcedHibernationDelay", "Minimale Pause nach einem erzwungenen Schlafzustand in Minuten", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(5)
            .addMaxValidator(60);

        // Gesetzt um die minimale Verweildauer im Schlafzustand zu unterdrücken.
        readonly ignoreMinSleep = new JMSLib.App.Flag({}, "suppressHibernationDelay", "Pause für erzwungenen Schlafzustand ignorieren");

        // Die Verweildauer eines Protokolleintrags vor der automatischen Löscung (in Wochen).
        readonly logKeep = new JMSLib.App.Number({}, "protocol", "Aufbewahrungsdauer für Protokolle in Wochen", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(1)
            .addMaxValidator(13);

        // Die Verweildauer eines Auftrags im Archiv vor der automatischen Löschung (in Wochen).
        readonly jobKeep = new JMSLib.App.Number({}, "archive", "Aufbewahrungsdauer von archivierten Aufzeichnungen in Wochen", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(1)
            .addMaxValidator(13);

        // Gesetzt, wenn die Systemzeit einer HDTV Aufzeichnung nicht automatisch ermittelt werden soll.
        readonly noH264PCR = new JMSLib.App.Flag({}, "noH264PCR", "Systemzeit (PCR) in Aufzeichnungsdateien nicht aus einem H.264 Bildsignal ableiten");

        // Gesetzt, wenn die Systemzeit einer SDTV Aufzeichnung nicht automatisch ermittelt werden soll.
        readonly noMPEG2PCR = new JMSLib.App.Flag({}, "noMPEG2PCR", "Systemzeit (PCR) in Aufzeichnungsdateien nicht aus einem MPEG2 Bildsignal ableiten");

        // Die Art des automatischen Schlafzustands.
        readonly hibernation = new JMSLib.App.SelectSingleFromList<HibernationMode>(this, "hibernationMode", "Art des von VCR.NET ausgelösten Schlafzustands", null, OtherSection._hibernation);

        // Die Art der Protokollierung.
        readonly logging = new JMSLib.App.SelectSingleFromList<string>({}, "logging", "Umfang der Protokollierung in das Windows Ereignisprotokoll", null, OtherSection._logging);

        // Fordert die Konfigurationswerte vom VCR.NET Recording Service an.
        protected loadAsync(): void {
            VCRServer.getOtherSettings().then(settings => {
                // Alle Präsentationsmodelle verbinden.
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

                // Die Anwendung kann nun verwendet werden.
                this.page.application.isBusy = false;

                // Anzeige aktualisieren lassen.
                this.refreshUi();
            });
        }

        // Die Beschriftung der Schaltfläche zum Speichern.
        protected readonly saveCaption = "Ändern und eventuell neu Starten";

        // Gesetzt, wenn ein Speichern möglich ist.
        protected get isValid(): boolean {
            // Alle Zahlen müssen fehlerfrei eingegeben worden sein - dazu gehört immer auch ein Wertebereich.
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

            // Ja, das geht.
            return true;
        }

        // Meldet oder ändert die Art des Schlafzustands.
        private get hibernationMode(): HibernationMode {
            var settings: VCRServer.OtherSettingsContract = this.port.data;

            // Die Art des Schlafzustands wird in den Daten über zwei Wahrheitswerte repräsentiert, wir machen es dem Anwender etwas einfacher.
            if (!settings)
                return null;
            else if (settings.mayHibernate)
                if (settings.useStandBy)
                    return HibernationMode.standBy;
                else
                    return HibernationMode.hibernate;
            else
                return HibernationMode.disabled;
        }

        private set hibernationMode(newMode: HibernationMode) {
            var settings: VCRServer.OtherSettingsContract = this.port.data;

            // Die Art des Schlafzustands in die beiden Wahrheitswerte umsetzen.
            if (settings) {
                settings.mayHibernate = (newMode !== HibernationMode.disabled);
                settings.useStandBy = (newMode === HibernationMode.standBy);
            }
        }

        // Beginnt die asynchrone Speicherung der Konfiguration.
        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            return VCRServer.setOtherSettings(this.port.data);
        }

    }
}