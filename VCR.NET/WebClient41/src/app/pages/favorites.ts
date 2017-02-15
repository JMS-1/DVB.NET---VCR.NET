/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface IFavoritesPage extends IPage {
    }

    export class FavoritesPage extends Page implements IFavoritesPage {
        constructor(application: Application) {
            super(`favorites`, application);
        }

        reset(sections: string[]): void {
            this.application.isBusy = false;
        }

        get title(): string {
            return `Gespeicherte Suchen`;
        }
   }
}