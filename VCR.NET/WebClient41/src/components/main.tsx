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
            var title = this._application.getTitle();

            if (document.title !== title)
                document.title = title;

            if (this.state && this.state.active)
                return <div className="vcrnet-main">
                    <h1>{this._application.page ? this._application.page.getTitle() : title}</h1>
                    <Navigation page={this._application.page} />
                    <View page={this._application.page} />
                </div>;
            else
                return <div className="vcrnet-main">
                    <h1>(Bitte etwas Geduld)</h1>
                </div>;
        }

        private onhashchange(): void {
            // Auslesen der Kennung - für FireFox ist es nicht möglich, .hash direkt zu verwenden, da hierbei eine Decodierung durchgeführt wird
            var query = window.location.href.split("#");
            var hash = (query.length > 1) ? query[1] : "";

            if (hash.length < 1)
                this.setPage();
            else {
                var sep = hash.indexOf(";");
                if (sep < 0)
                    this.setPage(hash);
                else
                    this.setPage(hash.substr(0, sep), hash.substr(sep + 1));
            }
        }

        private setPage(name: string = "", section: string = "") {
            this._application.switchPage(name, section);
        }
    }
}
