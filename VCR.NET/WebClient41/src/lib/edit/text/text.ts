/// <reference path="../edit.ts" />

namespace JMSLib.App {

    // Beschreibt eine Eigenschaft der Art Zeichenkette mit Prüfergebnissen.
    export interface IString extends IProperty<string> {
    }

    // Verwaltet eine Eigenschaft der Art Zeichenkette.
    export class String extends Property<string> implements IString {

        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, name: string, onChange: () => void, isRequired?: boolean, private readonly _defaultMessage?: string, validator?: (str: String) => string) {
            super(data, prop, name, onChange, isRequired, null, validator);
        }

        // Prüft den aktuellen Wert auf Gültigkeit.
        protected onValidate(): string {
            // Sollte die Basisklasse bereits einen Fehler melden so ist dieser so elementar, dass er unbedingt verwendet werden soll.
            var message = super.onValidate();

            if (message !== ``)
                return message;

            // Es gibt eine Einschränkung auf die Werte der Eigenschaft.
            if (this.isRequired) {
                // Der Wert darf nicht die leere Zeichenkette sein - und auch nicht nur aus Leerzeichen et al bestehen.
                var value = (this.value || "").trim();

                if (value.length < 1)
                    return this._defaultMessage || "Es muss ein Wert angegeben werden.";
            }

            // Ergebnis der Basisklasse nutzen.
            return message;
        }
    }
}