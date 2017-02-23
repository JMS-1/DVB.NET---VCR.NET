/// <reference path="page.ts" />
/// <reference path="../../lib/edit/list.ts" />
/// <reference path="../../lib/dateTimeUtils.ts" />

namespace VCRNETClient.App {

    // Interne Verwaltungseinheit für Konfigurationsbereiche.
    class SectionInfo implements Admin.ISectionInfo {

        // Erstellt eine neue Verwaltung.
        constructor(public readonly route: string, private readonly _factory: { new (section: AdminPage): Admin.Section }, private readonly _adminPage: AdminPage) {
        }

        // Die Präsentationsinstanz des zugehörigen Konfigurationsbereichs.
        private _section: Admin.Section;

        // Meldet die Präsentation des zugehörigen Konfigurationsbereichs - bei Bedarf wird eine neue erstellt.
        get section(): Admin.Section {
            // Beim ersten Aufruf eine neue Präsentationsinstanz anlegen.
            if (!this._section)
                this._section = new (this._factory)(this._adminPage);

            return this._section;
        }
    }

    // Schnittstelle zur Anzeige der Administration.
    export interface IAdminPage extends IPage {
        // Eine Auswahl für den aktuell anzuzeigenden Konfigurationsbereich.
        readonly sections: JMSLib.App.IValueFromList<Admin.ISectionInfo>;
    }

    // Das Präsentationsmodell für die Konfiguration des VCR.NET Recording Service.
    export class AdminPage extends Page implements IAdminPage {

        // Einmalig berechnet die Liste aller Stunden des Tages.
        static readonly hoursOfDay = Array.apply(null, Array(24)).map((d, i) => JMSLib.App.uiValue(i, JMSLib.App.DateTimeUtils.formatNumber(i)));

        // Die Liste aller Konfigurationsbereiche in der Reihenfolge, in der sie dem Anwender präsentiert werden sollen.
        private readonly _sections: JMSLib.App.IUiValue<SectionInfo>[] = [
            JMSLib.App.uiValue(new SectionInfo("security", Admin.SecuritySection, this), "Sicherheit"),
            JMSLib.App.uiValue(new SectionInfo("directories", Admin.DirectoriesSection, this), "Verzeichnisse"),
            JMSLib.App.uiValue(new SectionInfo("devices", Admin.DevicesSection, this), "Geräte"),
            JMSLib.App.uiValue(new SectionInfo("guide", Admin.GuideSection, this), "Programmzeitschrift"),
            JMSLib.App.uiValue(new SectionInfo("scan", Admin.ScanSection, this), "Quellen"),
            JMSLib.App.uiValue(new SectionInfo("rules", Admin.RulesSection, this), "Planungsregeln"),
            JMSLib.App.uiValue(new SectionInfo("other", Admin.OtherSection, this), "Sonstiges")
        ];

        // Präsentationsmodell zur Auswahl des aktuellen Konfigurationsbereichs.
        readonly sections: JMSLib.App.SelectSingleFromList<SectionInfo> = new JMSLib.App.SelectSingleFromList<SectionInfo>({}, "value", null, null, false, this._sections);

        // Erstellt ein neues Präsentationsmodell für die Seite.
        constructor(application: Application) {
            super("admin", application);
        }

        // Bereitet die Seite für die Anzeige vor.
        reset(sections: string[]): void {
            // Den aktuellen Konfigurationsbereich ermittelt - im Zweifel verwenden wir den ersten der Liste.
            var allSections = this.sections.allowedValues;
            var curSection = allSections.filter(v => v.value.route === sections[0])[0] || allSections[0];

            // Auswahl übernehmen.
            this.sections.value = curSection.value;

            // Den aktiven Konfigurationsbereich laden.
            curSection.value.section.reset();
        }

        // Aktualisiert eine Teilkonfiguration.
        update(request: JMSLib.App.IHttpPromise<boolean>, command: JMSLib.App.Command<void>): JMSLib.App.IHttpPromise<void> {
            // Auf das Ende der asynchronen Ausführung warten.
            return request.then(restartRequired => {
                if (restartRequired === true)
                    this.application.restart();
                else if (restartRequired === false)
                    this.application.gotoPage(null);
                else
                    command.message = `Ausführung zurzeit nicht möglich`;
            }, error => {
                // Fehlermeldung eintragen.
                command.message = error.message;

                // Weitere Fehlerbehandlung ermöglichen.
                return error;
            });
        }

        // Überschrift melden.
        get title(): string {
            return `Administration und Konfiguration`;
        }
    }
}