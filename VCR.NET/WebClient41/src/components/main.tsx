/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IMainStatic {
        application: App.Application;
    }

    interface IMainDynamic {
        version: string;

        msiVersion: string;
    }

    export class Main extends React.Component<IMainStatic, IMainDynamic>{
        render(): JSX.Element {
            var title = "VCR.NET Recording Service";

            if (this.state) {
                title = `${title} ${this.state.version}`;

                if (document.title !== title)
                    document.title = title;

                title = `${title} (${this.state.msiVersion})`;
            }

            return <div className="vcrnet-main">
                <h1>{title}</h1>
                <Router application={this.props.application} />
            </div>;
        }

        componentWillMount(): void {
            this.props.application.onNewServerVersion = info => this.setState({ version: info.version, msiVersion: info.msiVersion });
        }

        componentWillUnmount(): void {
            this.props.application.onNewServerVersion = undefined;
        }
    }
}
