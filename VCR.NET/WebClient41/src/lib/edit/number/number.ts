/// <reference path="../edit.ts" />

namespace JMSLib.App {

    // Beschreibt eine Eigenschaft mit einer Zahl mit Prüfergebnissen.
    export interface IValidatedNumber extends IValidatedValue<number> {
        rawValue: string;
    }

    // Verwaltet eine Eigenschaft mit einer Zahl.
    export class EditNumber extends EditValue<number> implements IValidatedNumber {
        private static readonly _positiveNumber = /^(0+|((0+)?[1-9][0-9]{0,5}))$/;

        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void, name: string) {
            super(data, prop, onChange, name);
        }

        private _rawInput: string;

        get rawValue(): string {
            if (this._rawInput === undefined)
                return this.value.toString();
            else
                return this._rawInput;
        }

        set rawValue(newValue: string) {
            var test = (newValue || ``).trim();

            if (EditNumber._positiveNumber.test(test)) {
                this._rawInput = undefined;
                this.value = parseInt(test);
            }
            else {
                this._rawInput = newValue;
            }

            this.validate();
            this.refresh();
        }

        validate(): void {
            super.validate();

            if (this.message.length > 0)
                return;

            if (this._rawInput !== undefined)
                this.message = `Ungültige Zahl`;
        }
    }
}