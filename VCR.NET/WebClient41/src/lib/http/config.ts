namespace JMSLib.App {

    export interface IHttpErrorInformation {
        readonly message: string;

        readonly details: string;
    }

    export interface IHttpPromise<TResponseType> extends Thenable<TResponseType, IHttpErrorInformation> {
    }

    export var webCallRoot: string;

    var webCallId = 0;

    export function nextWebCallId(): number {
        return ++webCallId;
    }

    export function switchView(): void {
        nextWebCallId();
    }
}