namespace VCRNETClient.App.NoUi {

    // Bietet den Wert einer Eigenschaft zur Pflege in der Oberfläche an-
    export interface IValueHolder<TValueType> extends JMSLib.App.IDisplayText {
        // Meldet den aktuellen Wert oder verändert diesen - gemeldet wird immer der ursprüngliche Wert.
        value: TValueType;

        // Gesetzt, wenn der Wert der Eigenschaft nicht verändert werden darf.
        readonly isReadonly: boolean;
    }

    // Ergänzt den Zugriff auf den Wert einer Eigenschaft um Prüfinformationen.
    export interface IValidatableValue<TValueType> extends IValueHolder<TValueType> {
        // Die zuletzt ermittelten Prüfinformationen. Eine leere Zeichenkette bedeutet, dass die Eigenschaft einen gültigen Wert besitzt.
        readonly message: string;
    }

    // Basisklasse zur Pflege des Wertes einer einzelnen Eigenschaft.
    export abstract class ValueHolder<TValueType> implements IValidatableValue<TValueType> {

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
        message: string;

        validate(): void {
            this.message = "";
        }

        // Initialisiert die Verwaltung des Wertes einer einzelnen Eigenschaft (_prop) im Modell (_data).
        protected constructor(private readonly _data: any, private readonly _prop: string, private readonly _onChange: () => void, public readonly text: string, private _testReadOnly?: () => boolean) {
        }
    }

    // Basisklasse zur Pflege des Wertes einer einzelnen Eigenschaft.
    export abstract class ValueHolderWithSite<TValueType> extends ValueHolder<TValueType> implements JMSLib.App.INoUiWithSite {
        private _site: JMSLib.App.INoUiSite;

        // Benachrichtigt die Oberfläche zur Aktualisierung der Anzeige.
        protected refresh(): void {
            if (this._site)
                this._site.refreshUi();
        }

        // Meldet die Oberfläche an.
        setSite(site: JMSLib.App.INoUiSite): void {
            this._site = site;

            if (this._site)
                this.onSiteChanged();
        }

        // Wird ausgelöst, wenn eine Anmeldung der Oberfläche erfolgt ist.
        protected onSiteChanged(): void {
        }
    }

}