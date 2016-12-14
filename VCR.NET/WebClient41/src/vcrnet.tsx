// Das muss noch geklärt werden: import geht leider nicht mit AMD und sonst können wir das nicht alles in eine Quelldatei bekommen.
import React = __React;
import ReactDOM = __React.__DOM;

namespace VCRNETClient {
    class Application {
        static startup(): void {
            ReactDOM.render(<Main />, document.querySelector(`body`));
        }
    }

    $(Application.startup);
}
