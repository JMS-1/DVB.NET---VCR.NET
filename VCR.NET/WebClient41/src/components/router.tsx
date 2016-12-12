/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IRouterStatic {
    }

    interface IRouterDynamic {
        view: string;

        subView: string;
    }

    export class Router extends React.Component<IRouterStatic, IRouterDynamic>{
        private _onhashchange: () => void;

        constructor() {
            super();

            this.state = { view: "", subView: "" };

            this._onhashchange = this.onhashchange.bind(this);
        }

        render(): JSX.Element {
            var active = <Home />;

            switch (this.state.view) {
                case "plan":
                    active = <Plan />;
                    break;
            }

            return <div className="vcrnet-router">{active}</div>;
        }

        private onhashchange(): void {
            var hash = document.location.hash || "";

            if ((hash.length < 1) || (hash[0] !== "#"))
                this.setState({ view: "", subView: "" });
            else {
                var sep = hash.indexOf(";");
                if (sep < 0)
                    this.setState({ view: hash.substr(1), subView: "" });
                else
                    this.setState({ view: hash.substr(1, sep - 1), subView: hash.substr(sep + 1) });
            }
        }

        componentDidMount(): void {
            window.addEventListener("hashchange", this._onhashchange);
        }

        componentWillUnmount(): void {
            window.removeEventListener("hashchange", this._onhashchange);
        }
    }
}
