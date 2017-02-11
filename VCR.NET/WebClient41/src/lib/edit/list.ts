/// <reference path="edit.ts" />

namespace JMSLib.App {

    // Beschreibt einen Wert zur Auswahl durch den Anwender.
    export interface IUiValue<TValueType> {
        // Der tatsächlich gespeicherte Wert.
        readonly value: TValueType;

        // Der Wert zur Anzeige.
        readonly display: string;
    }

    export function uiValue<TValueType>(value: TValueType, display?: string): IUiValue<TValueType> {
        return { value: value, display: (display === undefined) ? ((value === null) ? `` : value.toString()) : display };
    }

    export interface ISelectableUiValue<TValueType> extends IUiValue<TValueType> {
        selected: boolean;
    }

    export interface IValueFromList<TValueType> extends IProperty<TValueType> {
        readonly allowedValues: IUiValue<TValueType>[];

        displayValue: string;
    }

    export class EditFromList<TValueType> extends Property<TValueType> implements IValueFromList<TValueType> {

        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, name: string, onChange: () => void, isRequired?: boolean, private _allowedValues: IUiValue<TValueType>[] = []) {
            super(data, prop, name, onChange, isRequired);
        }

        get allowedValues(): IUiValue<TValueType>[] {
            return this._allowedValues;
        }

        set allowedValues(values: IUiValue<TValueType>[]) {
            this._allowedValues = values || [];
            this.refresh();
        }

        // Prüft den aktuellen Wert.
        validate(): void {
            // Sollte die Basisklasse bereits einen Fehler melden so ist dieser so elementar, dass er unbedingt verwendet werden soll.
            super.validate();

            if (this.message.length > 0)
                return;

            // Der Wert muss in der Liste sein.
            var value = this.value;

            if (value === null)
                if (!this.isRequired)
                    return;

            if (!this.allowedValues.some(av => av.value === value))
                this.message = "Der Wert ist nicht in der Liste der möglichen Werte enthalten.";
        }

        get displayValue(): string {
            var value = this.value;
            var display = this.allowedValues.filter(v => v.value === value);

            if (display.length === 1)
                return display[0].display;
            else
                return (value === null) ? null : value.toString();
        }

        set displayValue(newDisplay: string) {
            var value = this.allowedValues.filter(v => v.display === newDisplay);

            if (value.length === 1)
                this.value = value[0].value;
            else
                this.value = null;
        }
    }

}