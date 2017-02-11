/// <reference path="../edit.ts" />

namespace JMSLib.App {

    // Beschreibt eine Eigenschaft mit einer Zahl.
    export interface IEditNumber extends IProperty<number> {
        // Falls die Eingabe über einen Text erfolgt wird diese Eigenschaft zur Pflege verwendet.
        rawValue: string;
    }

    // Verwaltet eine Eigenschaft mit einer (ganzen) Zahl (mit maximal 6 Ziffern - ohne führende Nullen).
    export class EditNumber extends Property<number> implements IEditNumber {
        // Erlaubt sind beliebige Sequenzen von Nullen oder maximal 6 Ziffern - mit einer beliebigen Anzahl von führenden Nullen.
        private static readonly _positiveNumber = /^(0+|((0*)[1-9][0-9]{0,5}))$/;

        // Legt eine neue Verwaltung an.
        constructor(data?: any, prop?: string, name?: string, onChange?: () => void, isRequired?: boolean, private readonly _min?: number, private readonly _max?: number) {
            super(data, prop, name, onChange, isRequired);
        }

        // Entählt die aktuelle Fehleingabe.
        private _rawInput: string;

        // Meldet die aktuelle Eingabe - entweder eine Fehleingabe oder der Wert als Zeichenkette.
        get rawValue(): string {
            if (this._rawInput === undefined)
                return (this.value === null) ? `` : this.value.toString();
            else
                return this._rawInput;
        }

        // Übermittelt eine neue Eingabe.
        set rawValue(newValue: string) {
            var test = (newValue || ``).trim();

            if ((test.length < 1) && !this.isRequired) {
                this._rawInput = undefined;
                this.value = null;
            }
            else if (EditNumber._positiveNumber.test(test)) {
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

            else if (this.value === null) {
                if (this.isRequired)
                    this.message = `Es muss eine Zahl eingegeben werden`;
            }

            else if ((this._min !== undefined) && (this.value < this._min))
                this.message = `Die Zahl muss mindestens ${this._min} sein`;

            else if ((this._max !== undefined) && (this.value > this._max))
                this.message = `Die Zahl darf höchstens ${this._max} sein`;
        }
    }
}