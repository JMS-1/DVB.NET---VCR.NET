/// <reference path="section.ts" />

namespace VCRNETClient.App.Admin {

    // Schnittstelle zur Pflege der Planungsregeln.
    export interface IAdminRulesPage extends ISection {
        // Die Regeln zur Verteilung von Aufzeichnungen auf mehrere Geräte.
        readonly rules: JMSLib.App.IString;
    }

    // Präsentationsmodell zur Pflege der Planungsregeln.
    export class RulesSection extends Section implements IAdminRulesPage {

        // Der eindeutige Name des Bereichs.
        static readonly route = `rules`;

        // Die aktuellen Planungsregeln.
        readonly rules = new JMSLib.App.String({}, "rules");

        // Fordert die aktuellen Planungsregeln vom VCR.NET Recording Service an.
        protected loadAsync(): void {
            VCRServer.getSchedulerRules().then(settings => {
                // Konfiguration an das Präsentationsmodell binden.
                this.rules.data = settings;

                // Bedienung der Anwendung freischalten.
                this.page.application.isBusy = false;

                // Anzeige zur Aktualisierung auffordern.
                this.refreshUi();
            });
        }

        // Beschriftung der Schaltfläche zur Übernahme der Konfiguration.
        protected readonly saveCaption = "Ändern und neu Starten";

        // Konfiguration asynchron aktualisieren.
        protected saveAsync(): Promise<boolean> {
            return VCRServer.setSchedulerRules(this.rules.data);
        }

    }
}