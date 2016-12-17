/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IMainStatic {
    }

    interface IMainDynamic {
        active?: boolean;
    }

    export class Main extends React.Component<IMainStatic, IMainDynamic> implements App.IApplicationSite {
        private _application = new App.Application(this);

        private _onhashchange: () => void;

        constructor() {
            super();

            this._onhashchange = this.onhashchange.bind(this);
        }

        componentDidMount(): void {
            window.addEventListener("hashchange", this._onhashchange);
        }

        componentWillUnmount(): void {
            window.removeEventListener("hashchange", this._onhashchange);
        }

        onBusyChanged(isBusy: boolean): void {
            this.setState({ active: !isBusy });
        }

        onFirstStart(): void {
            this.onhashchange();
        }

        render(): JSX.Element {
            var title = "VCR.NET Recording Service";
            var version = this._application.version;

            if (version) {
                title = `${title} ${version.version}`;

                if (document.title !== title)
                    document.title = title;

                title = `${title} (${version.msiVersion})`;
            }

            if (this.state && this.state.active)
                return <div className="vcrnet-main">
                    <h1>{title}</h1>
                    <Navigation page={this._application.page} />
                    <View page={this._application.page} profile={this._application.profile} />
                </div>;
            else
                return <div className="vcrnet-main">
                    <h1>(Bitte etwas Geduld)</h1>
                </div>;
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
    }
}
