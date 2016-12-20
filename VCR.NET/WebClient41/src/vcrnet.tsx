// Das muss noch geklärt werden: import geht leider nicht mit AMD und sonst können wir das nicht alles in eine Quelldatei bekommen.
import React = __React;
import ReactDOM = __React.__DOM;

namespace VCRNETClient {
    // Initialisiert die react.js Laufzeitumgebung.
    $(() => ReactDOM.render(<Main />, document.querySelector(`vcrnet-spa`)));
}
