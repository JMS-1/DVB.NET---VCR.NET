﻿/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    export class View extends NoUiView<App.NoUi.IPage>{
        private static _pages: { [route: string]: { new (props?: INoUiComponent<any>, context?: any): NoUiView<App.NoUi.IPage> } };

        private static initializePages(application: App.IApplication): void {
            View._pages = {
                [application.homePage.route]: Home,
                [application.planPage.route]: Plan,
                [application.editPage.route]: Edit,
                [application.helpPage.route]: Help
            };
        }

        render(): JSX.Element {
            if (!View._pages)
                if (this.props.noui)
                    View.initializePages(this.props.noui.application);

            var factory = View._pages && View._pages[this.props.noui.route];

            return <div className="vcrnet-view">{factory ? React.createElement(factory, { noui: this.props.noui }) : null}</div>;
        }
    }
}
