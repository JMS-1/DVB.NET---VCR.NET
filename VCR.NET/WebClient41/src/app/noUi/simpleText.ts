namespace VCRNETClient.App.NoUi {

    // Beschreibt eine Eigenschaft der Art Zeichenkette mit Prüfergebnissen.
    export interface IStringEditor extends IValidatableValue<string> {
    }

    // Verwaltet eine Eigenschaft der Art Zeichenkette.
    export class StringEditor extends ValueHolder<string> implements IStringEditor {

        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void, private readonly _isRequired: boolean) {
            super(data, prop, onChange);
        }

        // Prüft den aktuellen Wert auf Gültigkeit.
        validate(): void {
            // Sollte die Basisklasse bereits einen Fehler melden so ist dieser so elementar, dass er unbedingt verwendet werden soll.
            super.validate();

            if (this.message.length > 0)
                return;

            // Es gibt eine Einschränkung auf die Werte der Eigenschaft.
            if (!this._isRequired)
                return;

            // Der Wert darf nicht die leere Zeichenkette sein - und auch nicht nur aus Leerzeichen et al bestehen.
            var value = (this.val() || "").trim();

            if (value.length < 1)
                this.message = "Es muss ein Wert angegeben werden.";
        }
    }
}