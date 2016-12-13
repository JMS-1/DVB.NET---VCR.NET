/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IMainStatic {
        application: App.Application;
    }

    interface IMainDynamic {
        serverVersion?: VCRServer.InfoServiceContract;
    }

    export class Main extends React.Component<IMainStatic, IMainDynamic>{
        render(): JSX.Element {
            var title = "VCR.NET Recording Service";

            if (this.state && this.state.serverVersion) {
                title = `${title} ${this.state.serverVersion.version}`;

                if (document.title !== title)
                    document.title = title;

                title = `${title} (${this.state.serverVersion.msiVersion})`;
            }

            return <div className="vcrnet-main">
                <h1>{title}</h1>
                <Router application={this.props.application} />
            </div>;
        }

        componentWillMount(): void {
            this.props.application.onNewServerVersion = info => this.setState({ serverVersion: info });
        }

        componentWillUnmount(): void {
            this.props.application.onNewServerVersion = undefined;
        }
    }
}
