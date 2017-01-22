/// <reference path="../../edit.ts" />

namespace JMSLib.App {

    // Beschreibt eine Eigenschaft mit einer Zahl mit Prüfergebnissen.
    export interface IValidatedNumber extends IValidatedValue<number> {
    }

    // Verwaltet eine Eigenschaft mit einer Zahl.
    export class EditNumber extends EditValue<number> implements IValidatedNumber {
        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void, name: string) {
            super(data, prop, onChange, name);
        }
    }
}