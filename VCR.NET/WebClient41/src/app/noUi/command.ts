/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    export interface ICommand extends IDisplayText, INoUiWithSite {
        readonly isEnabled: boolean;

        readonly isDangerous: boolean;

        execute(): void;
    }

    export class Command<TResponseType> implements ICommand {
        private _busy = false;

        constructor(private _begin: () => Thenable<TResponseType, XMLHttpRequest>, public text: string, private _test?: () => boolean) {
        }

        private _site: INoUiSite;

        setSite(newSite: INoUiSite): void {
            this._site = newSite;
        }

        private _dangerous = false;

        get isDangerous(): boolean {
            return this._dangerous;
        }

        set isDangerous(newValue: boolean) {
            if (newValue === this._dangerous)
                return;

            this._dangerous = newValue;

            this.refreshUi();
        }

        get isEnabled(): boolean {
            if (this._busy)
                return false;
            else if (this._test)
                return this._test();
            else
                return true;
        }

        private setBusy(newVal: boolean): void {
            if (this._busy === newVal)
                return;

            this._busy = newVal;

            this.refreshUi();
        }

        private refreshUi(): void {
            if (this._site)
                this._site.refreshUi();
        }

        execute(): void {
            if (!this.isEnabled)
                return;

            this.setBusy(true);

            this._begin().then(() => this.setBusy(false), () => this.setBusy(false));
        }
    }
}