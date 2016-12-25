namespace VCRNETClient.App.NoUi {

    // Beschreibt die Verwaltung einer beliebigen Eigenschaft.
    export interface IValueHolder<TValueType> {
        val(newValue?: TValueType): TValueType;
    }

    // Beschreibt eine beliebige Eigenschaft mit Prüfergebnissen.
    export interface IValidatableValue<TValueType> extends IValueHolder<TValueType> {
        readonly message: string;
    }

    // Verwaltet eine Eigenschaft und deren Prüfergebnisse.
    export abstract class ValueHolder<TValueType> implements IValidatableValue<TValueType> {

        // Aktuellen Wert auslesen oder verändert - es wird immer der bei Aufruf gültige Wert gemeldet.
        val(newValue?: TValueType): TValueType {
            var oldValue = this._data[this._prop] as TValueType;

            if (newValue !== undefined)
                if (newValue !== oldValue) {
                    // Aktualisieren.
                    this._data[this._prop] = newValue;

                    // Änderung melden, die Prüfung erfolgt extern.
                    this._onChange();
                }

            return oldValue;
        }

        // Verwaltung des Prüfergebnisses - die Basisimplementierung meldet die Eigenschaft immer als gültig (leere Zeichenkette).
        message: string;

        validate(): void {
            this.message = "";
        }

        // Initialisiert die Verwaltung einer Eigenschaft auf Basis eines JavaScript Feldes.
        protected constructor(private readonly _data: any, private readonly _prop: string, private readonly _onChange: () => void) {
        }
    }
}