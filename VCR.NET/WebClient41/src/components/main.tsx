/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IMainStatic {
        application: App.Application;
    }

    interface IMainDynamic {
        version: VCRServer.InfoServiceContract;
    }

    export class Main extends React.Component<IMainStatic, IMainDynamic>{
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
                <Router application={this.props.application} version={this.state && this.state.version} />
            </div>;
        }

        componentWillMount(): void {
            this.props.application.onNewServerVersion = info => this.setState({ version: info });
        }

        componentWillUnmount(): void {
            this.props.application.onNewServerVersion = undefined;
        }
    }
}
