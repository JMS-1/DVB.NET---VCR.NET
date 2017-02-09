namespace JMSLib.App {

    export interface IHttpErrorInformation {
        readonly message: string;

        readonly details: string;
    }

    export interface IHttpPromise<TResponseType> extends Thenable<TResponseType, IHttpErrorInformation> {
    }

    export var webCallRoot: string;

}