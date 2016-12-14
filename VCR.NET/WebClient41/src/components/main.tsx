/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IMainStatic {
    }

    interface IMainDynamic {
        version?: VCRServer.InfoServiceContract;

        active?: boolean;

        page?: App.Page;
    }

    export class Main extends React.Component<IMainStatic, IMainDynamic> implements App.IApplicationSite {
        private _application = new App.Application(this);

        private _onhashchange: () => void;

        constructor() {
            super();

            this._onhashchange = this.onhashchange.bind(this);
        }

        render(): JSX.Element {
            var title = "VCR.NET Recording Service";

            if (this.state) {
                title = `${title} ${this.state.version.version}`;

                if (document.title !== title)
                    document.title = title;

                title = `${title} (${this.state.version.msiVersion})`;
            }

            if (this.state && this.state.active)
                return <div className="vcrnet-main">
                    <h1>{title}</h1>
                    <Router application={this._application} version={this.state && this.state.version} view={this.state && this.state.page} />
                </div>;
            else
                return <div className="vcrnet-main">
                    <h1>(Bitte etwas Geduld)</h1>
                </div>;
        }

        onNewServerVersion(info: VCRServer.InfoServiceContract): void {
            this.setState({ version: info });
        }

        onBusyChanged(isBusy: boolean): void {
            this.setState({ active: !isBusy });
        }

        private onhashchange(): void {
            var hash = document.location.hash || "";

            if ((hash.length < 1) || (hash[0] !== "#"))
                this.setPage();
            else {
                var sep = hash.indexOf(";");
                if (sep < 0)
                    this.setPage(hash.substr(1));
                else
                    this.setPage(hash.substr(1, sep - 1), hash.substr(sep + 1));
            }
        }

        private setPage(name: string = "", section: string = "") {
            this._application.switchPage(name, section);
        }

        onNewPage(page: App.Page): void {
            this.setState({ page: page });
        }

        componentDidMount(): void {
            window.addEventListener("hashchange", this._onhashchange);
        }

        componentWillUnmount(): void {
            window.removeEventListener("hashchange", this._onhashchange);
        }
    }
}
