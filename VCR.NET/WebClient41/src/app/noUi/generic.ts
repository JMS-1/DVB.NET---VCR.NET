namespace VCRNETClient.App.NoUi {

    export interface IValueHolder<TValueType> {
        val(newValue?: TValueType): TValueType;
    }

    export interface IValidatableValue<TValueType> extends IValueHolder<TValueType> {
        readonly message: string;
    }

    export class ValueHolder<TValueType> implements IValidatableValue<TValueType> {
        val(newValue?: TValueType): TValueType {
            var oldValue = this._data[this._prop] as TValueType;

            if (newValue !== undefined)
                if (newValue !== oldValue) {
                    this._data[this._prop] = newValue;

                    this._onChange();
                }

            return oldValue;
        }

        message: string;

        validate(): void {
            this.message = "";
        }

        constructor(private _data: any, private _prop: string, private _onChange: () => void) {
        }
    }
}