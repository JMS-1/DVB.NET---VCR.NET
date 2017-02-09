namespace JMSLib.App {

    // Schnittstelle zur Anzeige einer Aktion.
    export interface ICommand extends IDisplayText, IConnectable {
        // Gesetzt, wenn die Aktion überhaupt angezeigt werden soll.
        readonly isVisible: boolean;

        // Gesetzt, wenn die Aktion zurzeit ausgeführt werden kann.
        readonly isEnabled: boolean;

        // Gesetzt, wenn die Aktion eine kritische Änderung bedeutet.
        readonly isDangerous: boolean;

        // Führt die Aktion aus.
        execute(): void;

        // Zugehörige Fehlermeldung.
        readonly message: string;
    }

    // Ui View Model zur Anzeige einer Aktion.
    export class Command<TResponseType> implements ICommand {

        // Gesetzt während die Aktion ausgeführt wird.
        private _busy = false;

        // Die zugehörige Anzeige.
        site: ISite;

        // Zugehörige Fehlermeldung.
        private _message = ``;

        get message(): string {
            return this._message;
        }

        set message(newMessage: string) {
            if (newMessage === this._message)
                return;

            this._message = newMessage;

            this.refreshUi();
        }

        // Erstellt eine neue Repräsentation.
        constructor(private _begin: () => (IHttpPromise<TResponseType> | void), public text: string, private _test?: () => boolean) {
        }

        // Gesetzt, wenn es sich um eine kritische Änderung handelt.
        private _dangerous = false;

        get isDangerous(): boolean {
            return this._dangerous;
        }

        set isDangerous(newValue: boolean) {
            // Nur Aktualisieren, wenn auch tatsächlich eine Umschaltung erfolgt ist.
            if (newValue === this._dangerous)
                return;

            this._dangerous = newValue;

            this.refreshUi();
        }

        // Gesetzt, wenn es sich um eine kritische Änderung handelt.
        private _visible = true;

        get isVisible(): boolean {
            return this._visible;
        }

        set isVisible(newValue: boolean) {
            // Nur Aktualisieren, wenn auch tatsächlich eine Umschaltung erfolgt ist.
            if (newValue === this._visible)
                return;

            this._visible = newValue;

            this.refreshUi();
        }

        // Gesetzt, wenn die Aktion ausgeführt werden darf.
        get isEnabled(): boolean {
            if (this._busy)
                return false;
            else if (!this.isVisible)
                return false;
            else if (this._test)
                return this._test();
            else
                return true;
        }

        // Ändert den Ausführungszustand der Aktion.
        private setBusy(newVal: boolean): void {
            // Oberfläche nur bei Änderungen aktualisieren.
            if (this._busy === newVal)
                return;

            this._busy = newVal;

            this.refreshUi();
        }

        // Oberfläche aktualiseren.
        private refreshUi(): void {
            if (this.site)
                this.site.refreshUi();
        }

        // Befehl ausführen.
        execute(): void {
            // Das ist im Mopment nicht möglich.
            if (!this.isEnabled)
                return;

            // Gegen erneutes Aufrufen sperren.
            this.setBusy(true);

            // Aktion starten.
            var begin = this._begin();

            // Auf das Ende der Aktion warten und Aktion wieder freigeben.
            var reenable: () => void = this.setBusy.bind(this, false);

            if (begin)
                begin.then(reenable, reenable);
            else
                reenable();
        }
    }
}