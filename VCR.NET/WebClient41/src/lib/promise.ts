namespace JMSLib.App {

    // Schnittstelle zur Verteilung asnychron ermittelter Daten.
    export interface Thenable<TResponseType, TErrorType> {
        // Meldet die Ergebnisauswertung an.
        then<TProjectedType>(onFulfilled?: (value: TResponseType) => TProjectedType | Thenable<TProjectedType, TErrorType> | void, onRejected?: (error: TErrorType) => TErrorType | Thenable<TProjectedType, TErrorType> | void): Thenable<TProjectedType, TErrorType>;
    }

    // Verteiler für asynchron ermittelte Daten.
    export class Promise<TResponseType, TErrorType> implements Thenable<TResponseType, TErrorType>{
        // Neu erstellen.
        constructor(executor: (onFulfilled: (value: TResponseType | Thenable<TResponseType, TErrorType> | void) => void, onRejected: (error: TErrorType | Thenable<TResponseType, TErrorType> | void) => void) => void) {
            executor(value => this.success(value), error => this.failure(error));
        }

        // Wird im Erfolgsfall aufgerufen.
        private _success: ((value: TResponseType) => void)[] = [];

        // Wird im Fehlerfall aufgerufen.
        private _failure: ((error: TErrorType) => void)[] = [];

        // Dasbereits bekannte Ergebnis.
        private _result: { isError: boolean; data?: TResponseType; error?: any };

        // Meldet die Ergebnisauswertung an.
        then<TProjectedType>(onFulfilled?: (value: TResponseType) => TProjectedType | Thenable<TProjectedType, TErrorType> | void, onRejected?: (error: TErrorType) => TErrorType | Thenable<TProjectedType, TErrorType> | void): Thenable<TProjectedType, TErrorType> {
            // Nachfolger erstellen.
            var next = new Promise<TProjectedType, TErrorType>((success, failure) => {
                // Nachfoler einbinden.
                if (onFulfilled)
                    this._success.push(v => success(onFulfilled(v)));
                if (onRejected)
                    this._failure.push(e => failure(onRejected(e)));
            });

            // Eventuell vorhandenes Ergebnis direkt verteilen.
            this.dispatch();

            // Nachfolger melden.
            return next;
        }

        // Ergebnis verteilen.
        private dispatch(): void {
            // Noch kein Ergebnis da.
            if (!this._result)
                return;

            // Je nach Situation verteilen.
            if (this._result.isError)
                this._failure.splice(0).forEach(c => c(this._result.error));
            else
                this._success.splice(0).forEach(c => c(this._result.data));
        }

        // Prüft, ob eine Weiterleitung verwendet werden soll.
        private getThenable(test: any): Thenable<any, TErrorType> {
            if (!test)
                return null;

            // Macht nur Sinn wenn das Objekt eine then Methode hat - hm, geht das nicht auch sicherer?
            var thenMethod = test["then"];
            if (!thenMethod)
                return null;
            if (!(thenMethod instanceof Function))
                return null;

            // Irgendwann später probieren wir es noch einmal.
            return (test as Thenable<any, TErrorType>);
        }

        // Daten melden.
        private success(value: TResponseType | Thenable<TResponseType, TErrorType> | void): void {
            // Das geht immer nur einmal.
            if (this._result)
                throw "Ein Promise kann nur einmal benachrichtigt werden";

            // Auf verzögerte Ausführung prüfen.
            var asAsync = this.getThenable(value);

            if (asAsync)
                asAsync.then(v => this.success(v));
            else {
                // Ergebnis merken und verteilen.
                this._result = { isError: false, data: value as TResponseType };

                this.dispatch();
            }
        }

        // Fehler melden.
        private failure(error: TErrorType | Thenable<TResponseType, TErrorType> | void): void {
            // Das geht immer nur einmal.
            if (this._result)
                throw "Ein Promise kann nur einmal benachrichtigt werden";

            // Auf verzögerte Ausführung prüfen.
            var asAsync = this.getThenable(error);

            if (asAsync)
                asAsync.then(e => this.failure(e));
            else {
                // Ergebnis merken und verteilen.
                this._result = { isError: true, error: error as TErrorType };

                this.dispatch();
            }
        }
    }

}