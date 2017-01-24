namespace JMSLib.App {

    // Bietet den Wert einer Eigenschaft zur Pflege in der Oberfläche an-
    export interface IEditValue<TValueType> extends IDisplayText {
        // Meldet den aktuellen Wert oder verändert diesen - gemeldet wird immer der ursprüngliche Wert.
        value: TValueType;

        // Gesetzt, wenn der Wert der Eigenschaft nicht verändert werden darf.
        readonly isReadonly: boolean;
    }

    // Ergänzt den Zugriff auf den Wert einer Eigenschaft um Prüfinformationen.
    export interface IValidatedValue<TValueType> extends IEditValue<TValueType> {
        // Die zuletzt ermittelten Prüfinformationen. Eine leere Zeichenkette bedeutet, dass die Eigenschaft einen gültigen Wert besitzt.
        readonly message: string;
    }

    // Basisklasse zur Pflege des Wertes einer einzelnen Eigenschaft.
    export abstract class EditValue<TValueType> implements IValidatedValue<TValueType> {

        // Meldet den aktuellen Wert oder verändert diesen - gemeldet wird immer der ursprüngliche Wert.
        get value(): TValueType {
            return this._data[this._prop] as TValueType;
        }

        set value(newValue: TValueType) {
            // Prüfen, ob der aktuelle Wert durch einen anderen ersetzt werden soll.
            if (newValue === this._data[this._prop])
                return;

            // Neuen Wert ins Modell übertragen.
            this._data[this._prop] = newValue;

            // Modelländerung melden.
            this._onChange();
        }

        // Meldet, ob der Wert der Eigenschaft nicht verändert werden darf.
        get isReadonly(): boolean {
            return this._testReadOnly && this._testReadOnly();
        }

        // Verwaltung des Prüfergebnisses - die Basisimplementierung meldet die Eigenschaft immer als gültig (leere Zeichenkette).
        message = "";

        validate(): void {
            this.message = "";
        }

        // Initialisiert die Verwaltung des Wertes einer einzelnen Eigenschaft (_prop) im Modell (_data).
        protected constructor(private readonly _data: any, private readonly _prop: string, private readonly _onChange: () => void, public readonly text: string, private _testReadOnly?: () => boolean) {
        }
    }

    // Basisklasse zur Pflege des Wertes einer einzelnen Eigenschaft.
    export abstract class EditValueWithSite<TValueType> extends EditValue<TValueType> implements IConnectable {
        private _site: ISite;

        // Benachrichtigt die Oberfläche zur Aktualisierung der Anzeige.
        protected refresh(): void {
            if (this._site)
                this._site.refreshUi();
        }

        // Meldet die Oberfläche an.
        setSite(site: ISite): void {
            this._site = site;

            if (this._site)
                this.onSiteChanged();
        }

        // Wird ausgelöst, wenn eine Anmeldung der Oberfläche erfolgt ist.
        protected onSiteChanged(): void {
        }
    }

}