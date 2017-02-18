/// <reference path="reactUi.tsx" />

namespace JMSLib.ReactUi {

    export interface IRoutablePage {
        route: string;
    }

    export interface IPageFactory<TPageType extends IRoutablePage> {
        [route: string]: { new (props?: JMSLib.ReactUi.IComponent<any>, context?: any): JMSLib.ReactUi.Component<TPageType> };
    }

    export abstract class Router<TPageType extends IRoutablePage> extends JMSLib.ReactUi.Component<TPageType>{
        private static _pages: IPageFactory<any>;

        protected abstract getPages(page: TPageType): IPageFactory<TPageType>;

        render(): JSX.Element {
            if (!Router._pages)
                if (this.props.noui)
                    Router._pages = this.getPages(this.props.noui);

            var factory = Router._pages && Router._pages[this.props.noui.route];

            return <div className="jmslib-router">{factory ? React.createElement(factory, { noui: this.props.noui }) : null}</div>;
        }
    }
}
