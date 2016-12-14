/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IRouterStatic {
        application: App.Application;
    }

    interface IRouterDynamic {
        view: string;
    }

    export class Router extends React.Component<IRouterStatic, IRouterDynamic>{
        private _onhashchange: () => void;

        constructor() {
            super();

            this.state = { view: "" };

            this._onhashchange = this.onhashchange.bind(this);
        }

        render(): JSX.Element {
            var active = <div />;

            switch (this.state.view) {
                case App.HomePage.name:
                    active = <Home page={this.props.application.getHomePage()} />
                    break;
                case App.PlanPage.name:
                    active = <Plan page={this.props.application.getPlanPage()} />
                    break;
            }

            return <div className="vcrnet-router">{active}</div>;
        }

        private onhashchange(): void {
            var hash = document.location.hash || "";

            if ((hash.length < 1) || (hash[0] !== "#"))
                this.props.application.setPage();
            else {
                var sep = hash.indexOf(";");
                if (sep < 0)
                    this.props.application.setPage(hash.substr(1));
                else
                    this.props.application.setPage(hash.substr(1, sep - 1), hash.substr(sep + 1));
            }
        }

        componentWillMount(): void {
            this.props.application.onNewPage = page => this.setState({ view: page.getName() });

            window.addEventListener("hashchange", this._onhashchange);

            this.onhashchange();
        }

        componentWillUnmount(): void {
            window.removeEventListener("hashchange", this._onhashchange);

            this.props.application.onNewPage = undefined;
        }
    }
}
