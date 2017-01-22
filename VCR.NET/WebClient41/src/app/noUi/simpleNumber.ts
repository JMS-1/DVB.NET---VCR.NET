/// <reference path="../../lib/edit.ts" />

namespace VCRNETClient.App.NoUi {

    // Beschreibt eine Eigenschaft mit einer Zahl mit Prüfergebnissen.
    export interface INumberEditor extends JMSLib.App.IValidatedValue<number> {
    }

    // Verwaltet eine Eigenschaft mit einer Zahl.
    export class NumberEditor extends JMSLib.App.EditValue<number> implements INumberEditor {
        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, onChange: () => void, name: string) {
            super(data, prop, onChange, name);
        }
    }
}