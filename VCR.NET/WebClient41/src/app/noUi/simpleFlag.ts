/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    // Beschreibt eine Eigenschaft mit einem Wahrheitswert mit Prüfergebnissen.
    export interface IBooleanEditor extends IValidatableValue<boolean> {
    }

    // Verwaltet eine Eigenschaft mit einem Wahrheitswert.
    export class BooleanEditor extends ValueHolder<boolean> implements IBooleanEditor {
        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void, name: string) {
            super(data, prop, onChange, name);
        }
    }

    // Verwaltet eine Eigenschaft mit ein Wahrheitswerten.
    export class BooleanSetEditor implements IBooleanEditor {
        message = "";

        // Legt eine neue Verwaltung an.
        constructor(private _mask: number, private readonly _flags: NumberEditor, public name: string) {
        }

        val(newValue?: boolean): boolean {
            var oldValue = this._flags.val();

            if (newValue !== undefined)
                if (newValue)
                    this._flags.val(oldValue | this._mask);
                else
                    this._flags.val(oldValue & ~this._mask);

            return ((oldValue & this._mask) !== 0);
        }
    }
}