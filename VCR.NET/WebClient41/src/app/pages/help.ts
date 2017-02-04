/// <reference path="page.ts" />

namespace VCRNETClient.App {

    // Informationen zu einzelnen Aspekten der Hilfe.
    export interface IHelpComponent {
    }

    // Ermittelt Informationen zu einzelnen Aspekten der Hilfe.
    export interface IHelpComponentProvider<TComponentType extends IHelpComponent> {
        readonly [section: string]: TComponentType;
    }

    // Wird von der Oberfläche zur Verfügung gestellt - die Aspekte der Hilfe sind nur dort und nicht in der Anwendungslogik abgebildet.
    export interface IHelpSite extends JMSLib.App.ISite {
        // Ermittelt die aktuelle Überschrift.
        getCurrentHelpTitle(section: string): string;
    }

    // Schnittstelle zum Zugriff auf die Hilfeseite.
    export interface IHelpPage extends IPage {
        // Der aktuell ausgewählte Aspekt.
        getHelpComponent<TComponentType extends IHelpComponent>(): TComponentType;
    }

    // Präsentationsdaten zur Darstellung eines Aspektes der Hilfe.
    export class HelpPage extends Page<IHelpSite> implements IHelpPage {
        // Erstellt ein neues Modell.
        constructor(application: Application) {
            super("faq", application);
        }

        // Der aktuell ausgewählte Aspekt.
        section: string;

        // Initialisiert die Hilfeseite mit einem Aspekt.
        reset(sections: string[]): void {
            this.section = sections[0];

            // Daten werden dabei nicht benötigt.
            setTimeout(() => this.application.setBusy(false), 0);
        }

        // Meldet die aktuelle Überschrift.
        get title(): string {
            var site = this.site;

            // Das weiß in dieser Implementierung nur die Oberfläche.
            return site && site.getCurrentHelpTitle(this.section);
        }

        // Der aktuell ausgewählte Aspekt.
        getHelpComponent<TComponentType extends IHelpComponent>(): TComponentType {
            var provider = this.application.getHelpComponentProvider<TComponentType>();

            return provider && provider[this.section];
        }
    }
}