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

    export interface IValueFromList<TValueType> extends IEditValue<TValueType> {
        readonly allowedValues: IUiValue<TValueType>[];
    }

    export class EditFromList<TValueType> extends EditValue<TValueType> implements IValueFromList<TValueType> {

        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void, name: string, public allowedValues: IUiValue<TValueType>[]) {
            super(data, prop, onChange, name);
        }
    }

}