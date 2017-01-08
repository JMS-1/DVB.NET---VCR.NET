/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IViewStatic {
        page: App.NoUi.IPage;

        topics: IHelpComponentProvider;
    }

    export class View extends React.Component<IViewStatic, INoDynamicState>{
        render(): JSX.Element {
            var active: JSX.Element = null;

            if (this.props.page instanceof App.NoUi.HomePage)
                active = <Home noui={this.props.page as App.NoUi.HomePage} />
            else if (this.props.page instanceof App.NoUi.PlanPage)
                active = <Plan noui={this.props.page as App.NoUi.PlanPage} />
            else if (this.props.page instanceof App.EditPage)
                active = <Edit noui={this.props.page as App.EditPage} />
            else if (this.props.page instanceof App.HelpPage)
                active = <Help noui={this.props.page as App.HelpPage} topics={this.props.topics} />

            return <div className="vcrnet-view">{active}</div>;
        }
    }
}
