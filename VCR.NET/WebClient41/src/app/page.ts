namespace VCRNETClient.App {
    export abstract class Page {
        abstract getName(): string;

        abstract reset(): void;

        readonly refresh: () => void;

        constructor(protected readonly application: Application) {
            this.refresh = this.onRefresh.bind(this);
        }

        showNavigation(): boolean {
            return true;
        }

        showRefresh(): boolean {
            return false;
        }

        showPlan(): boolean {
            return true;
        }

        showGuide(): boolean {
            return true;
        }

        showFavorites(): boolean {
            return false;
        }

        showNew(): boolean {
            return true;
        }

        showCurrent(): boolean {
            return true;
        }

        onRefresh(): void {
        }
    }
}