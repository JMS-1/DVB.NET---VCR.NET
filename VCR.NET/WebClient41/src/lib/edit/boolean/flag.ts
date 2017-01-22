/// <reference path="../../edit.ts" />

namespace JMSLib.App {

    // Schnittstelle zur Pflege einer Eigenschaft mit einem Wahrheitswert.
    export interface IValidateFlag extends IValidatedValue<boolean> {
    }

    // Verwaltet den Wahrheitswert in einer Eigenschaft - hier können wir uns vollständig auf die Implementierung der Basisklasse verlassen.
    export class FlagEditor extends EditValue<boolean> implements IValidateFlag {
        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void, name: string, testReadOnly?: () => boolean) {
            super(data, prop, onChange, name, testReadOnly);
        }
    }

    // Verwaltet ein Bitfeld von Wahrheitswerten in einer Eigenschaft mit einer Zahl als Wert.
    export class FlagSetEditor implements IValidateFlag {
        // Prüfungen werden hierbei nicht individuell unterstützt.
        readonly message = "";

        // Erstelle eine Verwaltungsinstanz auf Basis der Verwaltung der elementaren Zahl.
        constructor(private _mask: number, private readonly _flags: EditNumber, public text: string) {
        }

        // Meldet den aktuellen Wert oder verändert diesen.
        get value(): boolean {
            return ((this._flags.value & this._mask) !== 0);
        }

        set value(newValue: boolean) {
            // Änderung bitweise an die eigentliche Eigenschaft übertragen.
            if (newValue)
                this._flags.value |= this._mask;
            else
                this._flags.value &= ~this._mask;
        }

        // Gesetzt, wenn der Wert der Eigenschaft nicht verändert werden darf.
        get isReadonly(): boolean {
            return this._flags.isReadonly;
        }
    }
}