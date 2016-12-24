namespace VCRNETClient.App.NoUi {

    // Beschreibt eine Eigenschaft der Art Zeichenkette mit einer festen Liste von möglichen Werten.
    export interface IStringFromListEditor extends IStringEditor {
        // Die Liste der erlaubten Werten.
        readonly allowedValues: string[];
    }

    // Verwaltete eine Eigenschaft der Art Zeichenkette, deren mögliche Werte festgelegt sind.
    export class StringListEditor extends StringEditor implements IStringFromListEditor {

        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void, isRequired: boolean, public readonly allowedValues: string[]) {
            super(data, prop, onChange, isRequired);
        }

        // Prüft den aktuellen Wert.
        validate(): void {
            // Sollte die Basisklasse bereits einen Fehler melden so ist dieser so elementar, dass er unbedingt verwendet werden soll.
            super.validate();

            if (this.message.length > 0)
                return;

            // Der Wert muss in der Liste sein.
            var value = (this.val() || "").trim();

            if (this.allowedValues.indexOf(value) < 0)
                this.message = "Der Wert ist nicht in der Liste der möglichen Werte enthalten.";
        }
    }
}