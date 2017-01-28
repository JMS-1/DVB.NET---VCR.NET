/// <reference path='config.ts' />

namespace JMSLib.App {

    export function browserWebCall<TResponseType, TRequestType>(url: string, method: string = 'GET', request?: TRequestType): IHttpPromise<TResponseType> {
        return new Promise<TResponseType, IHttpErrorInformation>((success, failure) => {
            var xhr = new XMLHttpRequest();

            xhr.addEventListener("load", () => {
                if (xhr.status < 400)
                    if (xhr.status === 204)
                        success(undefined);
                    else
                        success(JSON.parse(xhr.responseText));
                else
                    failure(xhr);
            });

            xhr.open(method, webCallRoot + url);
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