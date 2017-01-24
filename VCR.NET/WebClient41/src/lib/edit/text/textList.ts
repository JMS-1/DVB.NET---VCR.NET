/// <reference path="text.ts" />

namespace JMSLib.App {

    // Beschreibt eine Eigenschaft der Art Zeichenkette mit einer festen Liste von möglichen Werten.
    export interface IValidateStringFromList extends IValidatedString {
        // Die Liste der erlaubten Werten.
        readonly allowedValues: IUiValue<string>[];
    }

    // Verwaltete eine Eigenschaft der Art Zeichenkette, deren mögliche Werte festgelegt sind.
    export class EditStringFromList extends EditString implements IValidateStringFromList {

        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void, name: string, isRequired: boolean, public allowedValues: IUiValue<string>[]) {
            super(data, prop, onChange, name, isRequired);
        }

        // Prüft den aktuellen Wert.
        validate(): void {
            // Sollte die Basisklasse bereits einen Fehler melden so ist dieser so elementar, dass er unbedingt verwendet werden soll.
            super.validate();

            if (this.message.length > 0)
                return;

            // Der Wert muss in der Liste sein.
            var value = (this.value || "").trim();

            if (!this.allowedValues.some(av => av.value === value))
                this.message = "Der Wert ist nicht in der Liste der möglichen Werte enthalten.";
        }
    }
}