namespace JMSLib.App {

    export interface IHttpErrorInformation {
    }

    export interface IHttpPromise<TResponseType> extends Thenable<TResponseType, IHttpErrorInformation> {
    }

    export var webCallRoot: string;

}