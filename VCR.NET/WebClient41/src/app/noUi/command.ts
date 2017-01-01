/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    export interface ICommand extends IDisplayText {
        isEnabled(): boolean;

        execute(): void;
    }

    export class Command implements ICommand {
        constructor(private _executor: () => void, public isEnabled: () => boolean, public text: string) {
        }

        execute(): void {
            if (!this.isEnabled())
                return;
        }
    }
}