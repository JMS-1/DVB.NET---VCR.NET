namespace VCRNETClient.App {

    // Hilfsklasse zur Emulation von ES6 Promises - die Implementierung ist weit von korrekt (in verschiedenen Aspekten), für uns reicht es aber so.
    export class PromiseHelper<TResponseType> implements Thenable<TResponseType>{
        // Wird im Erfolgsfall aufgerufen.
        private _success: (value: TResponseType) => any | Thenable<any>;

        // Wird im Fehlerfall aufgerufen.
        private _failure: (error: any) => any | Thenable<any>;

        // Alle Nachfolger.
        private _next: PromiseHelper<any>[] = [];

        // Meldet die Ergebnisauswertung an.
        then<TProjectedType>(onFulfilled?: (value: TResponseType) => TProjectedType | Thenable<TProjectedType>, onRejected?: (error: any) => TProjectedType | Thenable<TProjectedType>): Thenable<TProjectedType> {
            this._success = onFulfilled;
            this._failure = onRejected;

            // Nachfolger einrichten.
            var next = new PromiseHelper<TProjectedType>();

            this._next.push(next);

            return next;
        }

        // Prüft, ob eine Weiterleitung verwendet werden soll.
        private propagate(test: any): boolean {
            if (!test)
                return false;

            // Macht nur Sinn wenn das Objekt eine then Methode hat - hm, geht das nicht auch sicherer?
            var thenMethod = test["then"];
            if (!thenMethod)
                return false;
            if (!(thenMethod instanceof Function))
                return false;

            // Irgendwann später probieren wir es noch einmal.
            var thenable: Thenable<any> = test;

            // Wenn der asnychrone Zugriff denn dann durch ist, dann können wir das durchreichen.
            thenable.then(value => this._next.forEach(next => next.success(value)), error => this._next.forEach(next => next.failure(next)));

            return true;
        }

        // Daten melden.
        success(value: TResponseType): void {
            var processedValue = this._success ? this._success(value) : value;

            if (!this.propagate(processedValue))
                this._next.forEach(next => next.success(processedValue));
        }

        // Fehler melden.
        failure(error: any): void {
            var processedError = this._failure ? this._failure(error) : error;

            if (!this.propagate(processedError))
                this._next.forEach(next => next.failure(processedError));
        }
    }

}