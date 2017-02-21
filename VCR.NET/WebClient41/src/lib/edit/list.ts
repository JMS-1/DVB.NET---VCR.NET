/// <reference path="edit.ts" />

namespace JMSLib.App {

    // Beschreibt einen Wert zur Auswahl durch den Anwender.
    export interface IUiValue<TValueType> {
        // Der tatsächlich gespeicherte Wert.
        readonly value: TValueType;

        // Der Wert zur Anzeige.
        readonly display: string;

        // Meldet ob der Wert ausgewählt wurde.
        isSelected?: boolean;
    }

    // Hilfsmethode zum Erstellen eines Auswahlwertes.
    export function uiValue<TValueType>(value: TValueType, display?: string): IUiValue<TValueType> {
        return { value: value, display: (display === undefined) ? ((value === null) ? `` : value.toString()) : display };
    }

    // Schnittstelle zur Auswahl eines einzelnen Wertes aus einer Liste erlaubter Werte.
    export interface IValueFromList<TValueType> extends IProperty<TValueType> {
        // Die erlaubten Werte.
        readonly allowedValues: IUiValue<TValueType>[];

        // Die laufende Nummer des aktuell ausgewählte Wertes.
        valueIndex: number;
    }

    // Erlaubt die Auswahl eines einzelnen Wertes aus einer Liste erlaubter Werte.
    export class SelectSingleFromList<TValueType> extends Property<TValueType> implements IValueFromList<TValueType> {

        // Legt ein neues Präsentationsmodell an.
        constructor(data?: any, prop?: string, name?: string, onChange?: () => void, isRequired?: boolean, private _allowedValues: IUiValue<TValueType>[] = []) {
            super(data, prop, name, onChange, isRequired);
        }

        // Meldet die Liste der aktuell erlaubten Werte.
        get allowedValues(): IUiValue<TValueType>[] {
            return this._allowedValues;
        }

        // Legt die Liste der aktuell erlaubten Werte neu fest.
        set allowedValues(values: IUiValue<TValueType>[]) {
            this._allowedValues = values || [];

            // Anzeige erneuern.
            this.refresh();
        }

        // Prüft den aktuellen Wert.
        validate(): void {
            // Sollte die Basisklasse bereits einen Fehler melden so ist dieser so elementar, dass er unbedingt verwendet werden soll.
            super.validate();

            if (this.message.length > 0)
                return;

            // Der Wert muss in der Liste sein - sofern er nicht leer und gleichzeitig optional ist.
            var value = this.value;

            if (!value)
                if (!this.isRequired)
                    return;

            if (!this.allowedValues.some(av => av.value === value))
                this.message = "Der Wert ist nicht in der Liste der erlaubten Werte enthalten.";
        }

        // Meldet die laufende Nummer des Wertes in der Liste der erlaubten Werte - existiert ein solcher nicht, wird 0 gemeldet.
        get valueIndex(): number {
            for (var i = 0; i < this.allowedValues.length; i++)
                if (this.allowedValues[i].value === this.value)
                    return i;

            return 0;
        }

        // Ändert den Wert gemäß der laufenden Nummer in der Liste der erlaubten Werte.
        set valueIndex(newIndex: number) {
            this.value = this.allowedValues[newIndex].value;
        }
    }

}