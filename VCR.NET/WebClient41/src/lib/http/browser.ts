/// <reference path='config.ts' />

namespace JMSLib.App {

    export function browserWebCall<TResponseType, TRequestType>(url: string, method: string = 'GET', request?: TRequestType): IHttpPromise<TResponseType> {
        var nextId = nextWebCallId() + 1;

        return new Promise<TResponseType, IHttpErrorInformation>((success, failure) => {
            var raw = (url.substr(0, 7) === "http://");
            var xhr = new XMLHttpRequest();

            xhr.addEventListener("load", () => {
                if (nextWebCallId() != nextId)
                    return;

                if (xhr.status < 400)
                    if (xhr.status === 204)
                        success(undefined);
                    else if (raw)
                        success(<any>xhr.responseText);
                    else
                        success(JSON.parse(xhr.responseText));
                else {
                    var errorInfo = JSON.parse(xhr.responseText);

                    failure(<IHttpErrorInformation>{ message: errorInfo.Message || errorInfo.ExceptionMessage, details: errorInfo.MessageDetails });
                }
            });

            if (!raw)
                url = webCallRoot + url;

            xhr.open(method, url);
            xhr.setRequestHeader("accept", "application/json");

            if (request === undefined) {
                xhr.send();
            }
            else {
                xhr.setRequestHeader("content-type", "application/json");

                xhr.send(JSON.stringify(request));
            }
        });
    }

}