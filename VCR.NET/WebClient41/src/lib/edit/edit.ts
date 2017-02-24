﻿namespace JMSLib.App {

    // Bietet den Wert einer Eigenschaft zur Pflege in der Oberfläche an.
    export interface IProperty<TValueType> extends IDisplay, IConnectable {
        // Meldet den aktuellen Wert oder verändert diesen - gemeldet wird immer der ursprüngliche Wert.
        value: TValueType;

        // Gesetzt, wenn der Wert der Eigenschaft nicht verändert werden darf.
        readonly isReadonly: boolean;

        // Die zuletzt ermittelten Prüfinformationen. Eine leere Zeichenkette bedeutet, dass die Eigenschaft einen gültigen Wert besitzt.
        readonly message: string;
    }

    // Schnittstelle für Prüfalgorithmen.
    export interface IPropertyValidator<TValueType> {
        // Führt die Prüfung aus.
        (property: Property<TValueType>): string;
    }

    // Basisklasse zur Pflege des Wertes einer einzelnen Eigenschaft.
    export abstract class Property<TValueType> implements IProperty<TValueType> {

        // Initialisiert die Verwaltung des Wertes einer einzelnen Eigenschaft (_prop) im Modell (_data).
        protected constructor(private _data: any = {}, private readonly _prop: string = `value`, public readonly text: string = null, private readonly _onChange?: () => void, protected readonly isRequired?: boolean, private readonly _testReadOnly?: () => boolean, validator?: (property: Property<TValueType>) => string) {
            // Prüfalgorithmus merken.
            if (validator)
                this.addValidator(validator);
        }

        // Alle Prüfalgorithmen.
        private _validators: IPropertyValidator<TValueType>[] = [];

        // Vermerkt einen Prüfalgorithmus.
        addValidator(validator: IPropertyValidator<TValueType>): this {
            this._validators.push(validator);

            return this;
        }

        // Das zugehörige Oberflächenelement.
        private _site: ISite;

        // Benachrichtigt die Oberfläche zur Aktualisierung der Anzeige.
        protected refresh(): void {
            // Prüfergebnis aktualisieren.
            this.validate();

            // Präsentationsmodell über Änderungen informieren.
            if (this._onChange)
                this._onChange();

            // Unmittelbar verbundenes Oberflächenelement aktualisieren.
            if (this._site)
                this._site.refreshUi();
        }

        // Meldet die Oberfläche an.
        set site(site: ISite) {
            this._site = site;

            // Interne Benachrichtigung auslösen.
            if (this._site)
                this.onSiteChanged();
        }

        // Ermittelt das aktuell zugeordnete Oberflächenelement.
        get site(): ISite {
            return this._site;
        }

        // Wird ausgelöst, wenn eine Anmeldung der Oberfläche erfolgt ist.
        protected onSiteChanged(): void {
        }

        // Meldet den aktuellen Wert.
        get value(): TValueType {
            return this.data[this._prop] as TValueType;
        }

        // Verändert den aktuellen Wert.
        set value(newValue: TValueType) {
            // Prüfen, ob der aktuelle Wert durch einen anderen ersetzt werden soll.
            if (newValue === this.value)
                return;

            // Neuen Wert ins Modell übertragen.
            this.data[this._prop] = newValue;

            // Modelländerung melden und Oberfläche aktualisieren.
            this.refresh();
        }

        // Meldet, ob der Wert der Eigenschaft nicht verändert werden darf.
        get isReadonly(): boolean {
            return this._testReadOnly && this._testReadOnly();
        }

        // Verwaltung des Prüfergebnisses - die Basisimplementierung meldet die Eigenschaft immer als gültig (leere Zeichenkette).
        private _message = ``;

        get message(): string {
            return this._message;
        }

        validate(): void {
            this._message = this.onValidate();
        }

        protected onValidate(): string {
            // Alle Prüfalgorithmen durchgehen.
            for (var i = 0; i < this._validators.length; i++) {
                // Prüfung ausführen.
                var message = this._validators[i](this);
                if (message)
                    return message;
            }

            // Kein Fehler.
            return ``;
        }

        // Meldet das aktuell zugeordnete Modell.
        get data(): any {
            return this._data;
        }

        // Ändert das aktuell zugeordnete Modell.
        set data(newValue: any) {
            this._data = newValue;

            // Dadurch kann sich natürlich der aktuelle Wert verändert haben.
            this.refresh();
        }
    }
}