/// <reference path="edit.ts" />

namespace JMSLib.App {

    export interface IMultiValueFromList<TValueType> extends IEditValue<TValueType[]> {
        readonly values: ISelectableUiValue<TValueType>[];
    }

    class SelectableValue<TValueType> implements ISelectableUiValue<TValueType>{

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

    export class SelectFromList<TValueType> extends EditValue<TValueType[]> implements IMultiValueFromList<TValueType> {

        constructor(data: any, prop: string, onChange: () => void, name: string, values: IUiValue<TValueType>[]) {
            super(data, prop, onChange, name);

            this.setValues(values);
        }

        private _values: ISelectableUiValue<TValueType>[];

        get values(): ISelectableUiValue<TValueType>[] {
            return this._values;
        }

        setValues(values: IUiValue<TValueType>[]): void {
            this._values = values.map(v => new SelectableValue<TValueType>(v, this));
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