/// <reference path="page.ts" />
/// <reference path="../../lib/edit/list.ts" />
/// <reference path="../../lib/dateTimeUtils.ts" />

namespace VCRNETClient.App {

    // Interne Verwaltungseinheit für Konfigurationsbereiche.
    class SectionInfo implements Admin.ISectionInfoFactory {

        // Meldet den eindeutigen Namen des Konfigurationsbereichs.
        get route(): string {
            return this._factory.route;
        }

        // Erstellt eine neue Verwaltung.
        constructor(private readonly _factory: { new (section: AdminPage): Admin.Section; route: string; }) {
        }

        // Die Präsentationsinstanz des zugehörigen Konfigurationsbereichs.
        private _section: Admin.Section;

        // Meldet die Präsentation des zugehörigen Konfigurationsbereichs - bei Bedarf wird eine neue erstellt.
        getOrCreate(adminPage: AdminPage): Admin.Section {
            // Beim ersten Aufruf eine neue Präsentationsinstanz anlegen.
            if (!this._section)
                this._section = new (this._factory)(adminPage);

            return this._section;
        }
    }

    // Schnittstelle zur Anzeige der Administration.
    export interface IAdminPage extends IPage {
        // Eine Auswahl für den aktuell anzuzeigenden Konfigurationsbereich.
        readonly sections: JMSLib.App.IValueFromList<Admin.ISectionInfo>;

        // Erstellt eine Instanz des aktuellen Konfigurationsbereichs.
        getOrCreateCurrentSection(): Admin.ISection;
    }

    // Das Präsentationsmodell für die Konfiguration des VCR.NET Recording Service.
    export class AdminPage extends Page implements IAdminPage {

        // Einmalig berechnet die Liste aller Stunden des Tages.
        static readonly hoursOfDay: JMSLib.App.IUiValue<number>[] = Array.apply(null, Array(24)).map((d, i) => JMSLib.App.uiValue(i, JMSLib.App.DateTimeUtils.formatNumber(i)));

        // Die Liste aller Konfigurationsbereiche in der Reihenfolge, in der sie dem Anwender präsentiert werden sollen.
        private readonly _sections: JMSLib.App.IUiValue<SectionInfo>[] = [
            JMSLib.App.uiValue(new SectionInfo(Admin.SecuritySection), "Sicherheit"),
            JMSLib.App.uiValue(new SectionInfo(Admin.DirectoriesSection), "Verzeichnisse"),
            JMSLib.App.uiValue(new SectionInfo(Admin.DevicesSection), "Geräte"),
            JMSLib.App.uiValue(new SectionInfo(Admin.GuideSection), "Programmzeitschrift"),
            JMSLib.App.uiValue(new SectionInfo(Admin.ScanSection), "Quellen"),
            JMSLib.App.uiValue(new SectionInfo(Admin.RulesSection), "Planungsregeln"),
            JMSLib.App.uiValue(new SectionInfo(Admin.OtherSection), "Sonstiges")
        ];

        // Präsentationsmodell zur Auswahl des aktuellen Konfigurationsbereichs.
        readonly sections = new JMSLib.App.SelectSingleFromList({}, "value", null, null, this._sections);

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
            curSection.value.getOrCreate(this).reset();
        }

        // Aktualisiert eine Teilkonfiguration.
        update(request: Promise<boolean>, command: JMSLib.App.Command<void>): Promise<void> {
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

        // Erstellt eine Instanz des aktuellen Konfigurationsbereichs.
        getOrCreateCurrentSection(): Admin.ISection {
            return this.sections.value.getOrCreate(this);
        }

        // Überschrift melden.
        get title(): string {
            return `Administration und Konfiguration`;
        }
    }
}