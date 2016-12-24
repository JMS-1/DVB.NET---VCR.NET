namespace VCRNETClient.App.NoUi {

    export interface IStringEditor extends IValidatableValue<string> {
    }

    export class StringEditor extends ValueHolder<string> implements IStringEditor {
        constructor(data: any, prop: string, onChange: () => void, private _isRequired = false) {
            super(data, prop, onChange);
        }

        validate(): void {
            super.validate();

            if (this.message.length > 0)
                return;

            if (!this._isRequired)
                return;

            var value = (this.val() || "").trim();
            if (value.length < 1)
                this.message = "Es muss ein Wert angegeben werden.";
        }
    }
}