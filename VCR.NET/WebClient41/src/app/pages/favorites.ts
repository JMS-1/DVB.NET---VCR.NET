/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IFavoritesPage extends IPage {
    }

    interface ISavedGuideQuery {
        // Das zu berücksichtigende Gerät
        device: string;

        // Optional die Quelle
        source: string;

        // Der Text zur Suche in der üblichen Notation mit der Suchart als erstem Zeichen
        text: string;

        // Gesetzt, wenn nur im Titel gesucht werden soll
        titleOnly: boolean;

        // Die Art der zu berücksichtigenden Quelle
        sourceType: VCRServer.GuideSource;

        // Die Art der Verschlüsselung
        encryption: VCRServer.GuideEncryption;
    }

    export class FavoritesPage extends Page implements IFavoritesPage {

        private static _Entries: ISavedGuideQuery[];

        constructor(application: Application) {
            super(`favorites`, application);
        }

        reset(sections: string[]): void {
            if (!FavoritesPage._Entries)
                FavoritesPage._Entries = JSON.parse(this.application.profile.guideSearches || "[]");

            this.application.isBusy = false;
        }

        get title(): string {
            return `Gespeicherte Suchen`;
        }
    }
}