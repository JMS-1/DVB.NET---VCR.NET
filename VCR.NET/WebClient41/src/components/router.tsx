/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IRouterStatic {
        application: App.Application;

        view: App.Page;

        version: VCRServer.InfoServiceContract;
    }

    interface IRouterDynamic {
    }

    export class Router extends React.Component<IRouterStatic, IRouterDynamic>{
        render(): JSX.Element {
            var active: JSX.Element = null;

            if (this.props.view)
                switch (this.props.view.getName()) {
                    case App.HomePage.name:
                        active = <Home page={this.props.view} />
                        break;
                    case App.PlanPage.name:
                        active = <Plan page={this.props.view} />
                        break;
                }

            return <div className="vcrnet-router">{active}</div>;
        }
    }
}
