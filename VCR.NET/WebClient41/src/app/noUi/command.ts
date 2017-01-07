/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    export interface ICommand extends IDisplayText {
        isEnabled(): boolean;

        execute(): void;
    }

    export class Command<TResponseType> implements ICommand {
        private _busy = false;

        constructor(private _begin: () => Thenable<TResponseType, XMLHttpRequest>, private _test: () => boolean, private _onChange: () => void, public text: string) {
        }

        isEnabled(): boolean {
            if (this._busy)
                return false;
            else
                return this._test();
        }

        private setBusy(newVal: boolean): void {
            if (this._busy === newVal)
                return;

            this._busy = newVal;

            this._onChange();
        }

        execute(): void {
            if (!this.isEnabled())
                return;

            this.setBusy(true);

            this._begin().then(() => this.setBusy(false), e => this.setBusy(false));
        }
    }
}