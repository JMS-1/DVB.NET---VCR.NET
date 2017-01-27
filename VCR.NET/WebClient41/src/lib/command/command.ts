namespace JMSLib.App {

    export interface ICommand extends IDisplayText, IConnectable {
        readonly isVisible: boolean;

        readonly isEnabled: boolean;

        readonly isDangerous: boolean;

        execute(): void;
    }

    export class Command<TResponseType> implements ICommand {
        private _busy = false;

        constructor(private _begin: () => (Thenable<TResponseType, XMLHttpRequest> | void), public text: string, private _test?: () => boolean, public isVisible: boolean = true) {
        }

        private _site: ISite;

        setSite(newSite: ISite): void {
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

            var begin = this._begin();

            if (begin)
                begin.then(() => this.setBusy(false), () => this.setBusy(false));
            else
                this.setBusy(false);
        }
    }
}