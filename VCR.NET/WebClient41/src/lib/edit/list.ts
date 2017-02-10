/// <reference path="edit.ts" />

namespace JMSLib.App {

    // Beschreibt einen Wert zur Auswahl durch den Anwender.
    export interface IUiValue<TValueType> {
        // Der tatsächlich gespeicherte Wert.
        readonly value: TValueType;

        // Der Wert zur Anzeige.
        readonly display: string;
    }

    export interface ISelectableUiValue<TValueType> extends IUiValue<TValueType> {
        selected: boolean;
    }

    export interface IValueFromList<TValueType> extends IValidatedValue<TValueType> {
        readonly allowedValues: IUiValue<TValueType>[];
    }

    export class EditFromList<TValueType> extends EditValue<TValueType> implements IValueFromList<TValueType> {

        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void, name: string, isRequired: boolean, public allowedValues: IUiValue<TValueType>[]) {
            super(data, prop, onChange, name, isRequired);
        }

        // Prüft den aktuellen Wert.
        validate(): void {
            // Sollte die Basisklasse bereits einen Fehler melden so ist dieser so elementar, dass er unbedingt verwendet werden soll.
            super.validate();

            if (this.message.length > 0)
                return;

            // Der Wert muss in der Liste sein.
            var value = this.value;

            if (!this.allowedValues.some(av => av.value === value))
                this.message = "Der Wert ist nicht in der Liste der möglichen Werte enthalten.";
        }
    }

}