/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    // Beschreibt eine Eigenschaft mit einer Zahl mit Prüfergebnissen.
    export interface INumberEditor extends IValidatableValue<number> {
    }

    // Verwaltet eine Eigenschaft mit einer Zahl.
    export class NumberEditor extends ValueHolder<number> implements INumberEditor {
        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void, name: string) {
            super(data, prop, onChange, name);
        }
    }
}