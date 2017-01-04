/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IViewStatic {
        page: App.Page;

        faqs: IHelpComponentProvider;
    }

    interface IViewDynamic {
    }

    export class View extends React.Component<IViewStatic, IViewDynamic>{
        render(): JSX.Element {
            var active: JSX.Element = null;

            if (this.props.page instanceof App.HomePage)
                active = <Home page={this.props.page as App.HomePage} />
            else if (this.props.page instanceof App.PlanPage)
                active = <Plan noui={this.props.page as App.PlanPage} />
            else if (this.props.page instanceof App.EditPage)
                active = <Edit page={this.props.page as App.EditPage} />
            else if (this.props.page instanceof App.HelpPage)
                active = <Help page={this.props.page as App.HelpPage} faqs={this.props.faqs} />

            return <div className="vcrnet-view">{active}</div>;
        }
    }
}
