/// <reference path="page.ts" />

namespace VCRNETClient.App {

    // Informationen zu einzelnen Aspekten der Hilfe.
    export interface IHelpComponent {
        readonly title: string;
    }

    // Ermittelt Informationen zu einzelnen Aspekten der Hilfe.
    export interface IHelpComponentProvider<TComponentType extends IHelpComponent> {
        readonly[section: string]: TComponentType;
    }

    // Schnittstelle zum Zugriff auf die Hilfeseite.
    export interface IHelpPage extends IPage {
        // Der aktuell ausgewählte Aspekt.
        getHelpComponent<TComponentType extends IHelpComponent>(): TComponentType;
    }

    // Präsentationsdaten zur Darstellung eines Aspektes der Hilfe.
    export class HelpPage extends Page implements IHelpPage {
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
            this.application.isBusy = false;
        }

        // Meldet die aktuelle Überschrift.
        get title(): string {
            var topic = this.getHelpComponent<IHelpComponent>();

            return topic && topic.title;
        }

        // Der aktuell ausgewählte Aspekt.
        getHelpComponent<TComponentType extends IHelpComponent>(): TComponentType {
            var provider = this.application.getHelpComponentProvider<TComponentType>();

            return provider && provider[this.section];
        }
    }
}