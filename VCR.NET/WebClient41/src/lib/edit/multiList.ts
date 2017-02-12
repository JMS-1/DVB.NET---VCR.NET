/// <reference path="edit.ts" />

namespace JMSLib.App {

    export interface IMultiValueFromList<TValueType> extends IProperty<TValueType[]> {
        readonly allowedValues: IUiValue<TValueType>[];
    }

    class SelectableValue<TValueType> implements IUiValue<TValueType>{

        constructor(value: IUiValue<TValueType>, private readonly _onChange: () => void) {
            this.selected = value.isSelected || false;
            this.display = value.display;
            this.value = value.value;
        }

        selected: boolean;

        get isSelected(): boolean {
            return this.selected;
        }

        set isSelected(newValue: boolean) {
            if (newValue === this.selected)
                return;

            this.selected = newValue;

            this._onChange();
        }

        readonly display: string;

        readonly value: TValueType;
    }

    export class SelectMultipleFromList<TValueType> extends Property<TValueType[]> implements IMultiValueFromList<TValueType> {

        constructor(data?: any, prop?: string, name?: string, onChange?: () => void, allowedValues: IUiValue<TValueType>[] = []) {
            super(data, prop, name, onChange);

            this.allowedValues = allowedValues;
        }

        private _allowedValues: SelectableValue<TValueType>[];

        get allowedValues(): IUiValue<TValueType>[] {
            return this._allowedValues;
        }

        set allowedValues(newValues: IUiValue<TValueType>[]) {
            this._allowedValues = newValues.map(v => new SelectableValue<TValueType>(v, () => this.setValueFromSelection()));

            this.setValueFromSelection();

            this.refresh();
        }

        private setValueFromSelection(): void {
            this.value = this.allowedValues.filter(v => v.isSelected).map(v => v.value);
        }

        refresh(): void {
            var values = this.value || [];

            this._allowedValues.forEach(av => av.selected = values.some(v => v === av.value));

            super.refresh();
        }
    }
}