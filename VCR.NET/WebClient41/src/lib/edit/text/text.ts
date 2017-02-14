/// <reference path="../edit.ts" />

namespace JMSLib.App {

    // Beschreibt eine Eigenschaft der Art Zeichenkette mit Prüfergebnissen.
    export interface IString extends IProperty<string> {
    }

    // Verwaltet eine Eigenschaft der Art Zeichenkette.
    export class String extends Property<string> implements IString {

        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, name: string, onChange: () => void, isRequired?: boolean, private _message?: string) {
            super(data, prop, name, onChange, isRequired);
        }

        // Prüft den aktuellen Wert auf Gültigkeit.
        validate(): void {
            // Sollte die Basisklasse bereits einen Fehler melden so ist dieser so elementar, dass er unbedingt verwendet werden soll.
            super.validate();

            if (this.message.length > 0)
                return;

            // Es gibt eine Einschränkung auf die Werte der Eigenschaft.
            if (!this.isRequired)
                return;

            // Der Wert darf nicht die leere Zeichenkette sein - und auch nicht nur aus Leerzeichen et al bestehen.
            var value = (this.value || "").trim();

            if (value.length < 1)
                this.message = this._message || "Es muss ein Wert angegeben werden.";
        }
    }
}