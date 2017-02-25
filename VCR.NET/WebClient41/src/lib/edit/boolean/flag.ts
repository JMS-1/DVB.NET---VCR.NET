/// <reference path="../edit.ts" />

namespace JMSLib.App {

    // Schnittstelle zur Pflege einer Eigenschaft mit einem Wahrheitswert.
    export interface IFlag extends IProperty<boolean> {
    }

    // Verwaltet den Wahrheitswert in einer Eigenschaft - hier können wir uns vollständig auf die Implementierung der Basisklasse verlassen.
    export class Flag extends Property<boolean> implements IFlag {
        // Legt eine neue Verwaltung an.
        constructor(data?: any, prop?: string, name?: string, onChange?: () => void, testReadOnly?: () => boolean) {
            super(data, prop, name, onChange, testReadOnly);
        }
    }

    // Verwaltet ein Bitfeld von Wahrheitswerten in einer Eigenschaft mit einer Zahl als Wert.
    export class FlagSet implements IFlag {
        // Prüfungen werden hierbei nicht individuell unterstützt.
        readonly message = ``;

        // Erstelle eine Verwaltungsinstanz auf Basis der Verwaltung der elementaren Zahl.
        constructor(private _mask: number, private readonly _flags: Number, public text: string) {
        }

        // Das zugehörige Oberflächenelement.
        site: ISite;

        // Meldet den aktuellen Wert oder verändert diesen.
        get value(): boolean {
            return ((this._flags.value & this._mask) !== 0);
        }

        set value(newValue: boolean) {
            // Änderung bitweise an die eigentliche Eigenschaft übertragen.
            var flags = newValue ? (this._flags.value | this._mask) : (this._flags.value & ~this._mask);

            // Keine Änderung.
            if (flags === this._flags.value)
                return;

            // Änderung durchführen.
            this._flags.value = flags;

            // Oberfläche aktualisieren.
            if (this.site)
                this.site.refreshUi();
        }

        // Gesetzt, wenn der Wert der Eigenschaft nicht verändert werden darf.
        get isReadonly(): boolean {
            return this._flags.isReadonly;
        }
    }
}