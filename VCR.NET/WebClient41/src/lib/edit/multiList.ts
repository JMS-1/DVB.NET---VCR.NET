/// <reference path="edit.ts" />

namespace JMSLib.App {

    export interface IMultiValueFromList<TValueType> extends IProperty<TValueType[]> {
        readonly values: IUiValue<TValueType>[];
    }

    class SelectableValue<TValueType> implements IUiValue<TValueType>{

        constructor(value: IUiValue<TValueType>, private readonly _list: SelectFromList<TValueType>) {
            this.display = value.display;
            this.value = value.value;
        }

        get selected(): boolean {
            return this._list.value.indexOf(this.value) >= 0;
        }

        set selected(newValue: boolean) {
            var values = this._list.value.filter(v => v !== this.value);

            if (newValue)
                values.push(this.value);

            this._list.value = values;
        }

        readonly display: string;

        readonly value: TValueType;
    }

    export class SelectFromList<TValueType> extends Property<TValueType[]> implements IMultiValueFromList<TValueType> {

        constructor(data: any, prop: string, name: string, onChange: () => void, values: IUiValue<TValueType>[]) {
            super(data, prop, name, onChange);

            this.setValues(values);
        }

        private _values: IUiValue<TValueType>[];

        get values(): IUiValue<TValueType>[] {
            return this._values;
        }

        setValues(values: IUiValue<TValueType>[]): void {
            this._values = values.map(v => new SelectableValue<TValueType>(v, this));
        }

        addValue(value: IUiValue<TValueType>): void {
            this._values.push(new SelectableValue<TValueType>(value, this));

            this.refresh();
        }

        get allValues(): TValueType[] {
            return this.values.map(v => v.value);
        }

        removeSelected(): void {
            this.setValues(this.values.filter(v => !v.selected));

            this.value = [];
        }
    }
}