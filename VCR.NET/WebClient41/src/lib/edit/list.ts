/// <reference path="edit.ts" />

namespace JMSLib.App {

    // Beschreibt einen Wert zur Auswahl durch den Anwender.
    export interface IUiValue<TValueType> {
        // Der tatsächlich gespeicherte Wert.
        readonly value: TValueType;

        // Der Wert zur Anzeige.
        readonly display: string;
    }

    export interface IValueFromList<TValueType> extends IEditValue<TValueType> {
        readonly allowedValues: IUiValue<TValueType>[];
    }

    export interface IMultiValueFromList<TValueType> extends IEditValue<TValueType[]> {
        readonly values: IUiValue<TValueType>[];

        toggle(value: TValueType): void;
    }

    export class EditFromList<TValueType> extends EditValue<TValueType> implements IValueFromList<TValueType> {

        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void, name: string, public allowedValues: IUiValue<TValueType>[]) {
            super(data, prop, onChange, name);
        }
    }

    export class SelectFromList<TValueType> extends EditValue<TValueType[]> implements IMultiValueFromList<TValueType> {

        constructor(data: any, prop: string, onChange: () => void, name: string, public values: IUiValue<TValueType>[]) {
            super(data, prop, onChange, name);
        }

        get allValues(): TValueType[] {
            return this.values.map(v => v.value);
        }

        toggle(value: TValueType): void {
            var list = this.value.filter(v => v !== value);

            list.push(value);

            this.value = list;
        }
    }
}