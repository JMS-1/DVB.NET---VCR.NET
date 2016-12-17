/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IViewStatic {
        page: App.Page;

        profile: VCRServer.UserProfileContract;
    }

    interface IViewDynamic {
    }

    export class View extends React.Component<IViewStatic, IViewDynamic>{
        render(): JSX.Element {
            var active: JSX.Element = null;

            if (this.props.page)
                switch (this.props.page.getName()) {
                    case App.HomePage.name:
                        active = <Home page={this.props.page as App.HomePage} />
                        break;
                    case App.PlanPage.name:
                        active = <Plan page={this.props.page as App.PlanPage} daysToShow={this.props.profile.planDays} />
                        break;
                }

            return <div className="vcrnet-view">{active}</div>;
        }
    }
}
