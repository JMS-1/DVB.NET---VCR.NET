/// <reference path="page.ts" />

namespace VCRNETClient.App.NoUi {

    // Wird von der Oberfläche zur Verfügung gestellt - die Aspekte der Hilfe sind nur dort und nicht in der Anwendungslogik abgebildet.
    export interface IHelpSite extends NoUi.IPageSite {
        // Ermittelt die aktuelle Überschrift.
        getCurrentHelpTitle(section: string): string;
    }

    // Schnittstelle zum Zugriff auf die Hilfeseite.
    export interface IHelpPage extends IPage {
        // Der aktuell ausgewählte Aspekt.
        readonly section: string;
    }

    // Präsentationsdaten zur Darstellung eines Aspektes der Hilfe.
    export class HelpPage extends NoUi.Page<IHelpSite> implements IHelpPage {
        // Erstellt ein neues Modell.
        constructor(application: Application) {
            super("faq", application);
        }

        // Der aktuell ausgewählte Aspekt.
        section: string;

        // Initialisiert die Hilfeseite mit einem Aspekt.
        reset(section: string): void {
            this.section = section;

            // Daten werden dabei nicht benötigt.
            setTimeout(() => this.application.setBusy(false), 0);
        }

        // Meldet die aktuelle Überschrift.
        getTitle(): string {
            var site = this.getSite();

            // Das weiß in dieser Implementierung nur die Oberfläche.
            return site && site.getCurrentHelpTitle(this.section);
        }
    }
}