/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IAdminScanPage extends IAdminSection {
    }

    export class ScanSection extends AdminSection implements IAdminScanPage {

        reset(): void {
            this.page.application.isBusy = false;
        }
    }
}