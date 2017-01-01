/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    // Schnittstelle zur Pflege einer Eigenschaft mit einem Wahrheitswert.
    export interface IBooleanEditor extends IValidatableValue<boolean> {
    }

    // Verwaltet den Wahrheitswert in einer Eigenschaft - hier können wir uns vollständig auf die Implementierung der Basisklasse verlassen.
    export class BooleanEditor extends ValueHolder<boolean> implements IBooleanEditor {
        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void, name: string, testReadOnly?: () => boolean) {
            super(data, prop, onChange, name, testReadOnly);
        }
    }

    // Verwaltet ein Bitfeld von Wahrheitswerten in einer Eigenschaft mit einer Zahl als Wert.
    export class BooleanSetEditor implements IBooleanEditor {
        // Prüfungen werden hierbei nicht individuell unterstützt.
        readonly message = "";

        // Erstelle eine Verwaltungsinstanz auf Basis der Verwaltung der elementaren Zahl.
        constructor(private _mask: number, private readonly _flags: NumberEditor, public text: string) {
        }

        // Meldet den aktuellen Wert oder verändert diesen.
        val(newValue?: boolean): boolean {
            // Ursprüngliches Bitfeld auslesen.
            var oldValue = this._flags.val();

            // Änderung bitweise an die eigentliche Eigenschaft übertragen.
            if (newValue !== undefined)
                if (newValue)
                    this._flags.val(oldValue | this._mask);
                else
                    this._flags.val(oldValue & ~this._mask);

            // Zustand gemäß des ursprünglichen Bitfelds auslesen.
            return ((oldValue & this._mask) !== 0);
        }

        // Gesetzt, wenn der Wert der Eigenschaft nicht verändert werden darf.
        isReadonly(): boolean {
            return this._flags.isReadonly();
        }
    }
}