/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IMainStatic {
    }

    interface IMainDynamic {
        serverVersion?: VCRServer.InfoServiceContract;
    }

    export class Main extends React.Component<IMainStatic, IMainDynamic>{
        render(): JSX.Element {
            var title = Strings.headline.productName;

            if (this.state && this.state.serverVersion) {
                title = `${title} ${this.state.serverVersion.version}`;

                if (document.title !== title)
                    document.title = title;

                title = `${title} (${this.state.serverVersion.msiVersion})`;
            }

            return <div className="vcrnet-main">
                <h1>{title}</h1>
                <Router />
            </div>;
        }

        componentDidMount(): void {
            VCRServer.getServerVersion().done(info => this.setState({ serverVersion: info }));
        }
    }
}
