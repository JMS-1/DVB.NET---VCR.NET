namespace VCRNETClient.App.NoUi {

    // Beschreibt eine Eigenschaft mit einem Wahrheitswert mit Prüfergebnissen.
    export interface IBooleanEditor extends IValidatableValue<boolean> {
    }

    // Verwaltet eine Eigenschaft mit Wahrheitswerten.
    export class BooleanEditor extends ValueHolder<boolean> implements IBooleanEditor {
        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void) {
            super(data, prop, onChange);
        }
    }
}