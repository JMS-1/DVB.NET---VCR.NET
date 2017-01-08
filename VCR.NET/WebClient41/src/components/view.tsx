/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    export class View extends NoUiView<App.NoUi.IPage>{
        render(): JSX.Element {
            var active: JSX.Element = null;

            // Das ist noch SEHR unschön, da müssen wir noch einmal ran!
            if (this.props.noui instanceof App.NoUi.HomePage)
                active = <Home noui={this.props.noui as App.NoUi.HomePage} />
            else if (this.props.noui instanceof App.NoUi.PlanPage)
                active = <Plan noui={this.props.noui as App.NoUi.PlanPage} />
            else if (this.props.noui instanceof App.EditPage)
                active = <Edit noui={this.props.noui as App.EditPage} />
            else if (this.props.noui instanceof App.NoUi.HelpPage)
                active = <Help noui={this.props.noui as App.NoUi.IHelpPage} />

            return <div className="vcrnet-view">{active}</div>;
        }
    }
}
