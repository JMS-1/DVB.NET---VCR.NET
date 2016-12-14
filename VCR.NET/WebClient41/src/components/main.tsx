/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IMainStatic {
    }

    interface IMainDynamic {
        version: VCRServer.InfoServiceContract;
    }

    export class Main extends React.Component<IMainStatic, IMainDynamic> implements App.IApplicationSite {
        private _application = new App.Application(this);

        render(): JSX.Element {
            var title = "VCR.NET Recording Service";

            if (this.state) {
                title = `${title} ${this.state.version.version}`;

                if (document.title !== title)
                    document.title = title;

                title = `${title} (${this.state.version.msiVersion})`;
            }

            return <div className="vcrnet-main">
                <h1>{title}</h1>
                <Router application={this._application} version={this.state && this.state.version} />
            </div>;
        }

        onNewServerVersion(info: VCRServer.InfoServiceContract): void {
            this.setState({ version: info });
        }
    }
}
