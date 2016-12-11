// Das muss noch geklärt werden: import geht leider nicht mit AMD und sonst können wir das nicht alles in eine Quelldatei bekommen.
import React = __React;
import ReactDOM = __React.__DOM;

namespace VCRNETClient {
    export interface IApplication {
    }

    class Main extends React.Component<{}, {}>{
        render(): JSX.Element {
            return <div>[TBD]</div>;
        }
    }

    class Application implements IApplication {
        constructor() {
            $(this.startup.bind(this));
        }

        private startup(): void {
            ReactDOM.render(<Main />, document.querySelector(`body`));
        }
    }

    export var App: IApplication = new Application();
}
