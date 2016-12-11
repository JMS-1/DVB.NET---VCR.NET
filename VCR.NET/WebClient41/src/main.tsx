/// <reference path="viewModels/main.ts" />

// Das muss noch geklärt werden: import geht leider nicht mit AMD und sonst können wir das nicht alles in eine Quelldatei bekommen.
import React = __React;
import ReactDOM = __React.__DOM;

namespace VCRNETClient {
    export interface IApplication {
        data: ApplicationViewModel;
    }

    interface IMain {
        vm: ApplicationViewModel;
    }

    class Main extends React.Component<IMain, {}>{
        render(): JSX.Element {
            return <div>{this.props.vm.currentVersion()}</div>;
        }
    }

    class Application implements IApplication {
        data = new ApplicationViewModel();

        constructor() {
            $(this.startup.bind(this));
        }

        private startup(): void {
            ReactDOM.render(<Main vm={this.data}/>, document.querySelector(`body`));
        }
    }

    export var App: IApplication = new Application();
}
