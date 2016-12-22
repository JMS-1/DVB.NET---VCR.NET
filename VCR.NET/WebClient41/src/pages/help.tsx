/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IHelpStatic {
        page: App.HelpPage;

        faqs: IHelpComponentProvider;
    }

    interface IHelpDynamic {
    }

    export class Help extends React.Component<IHelpStatic, IHelpDynamic> {
        render(): JSX.Element {
            var element = this.props.faqs.getHelpComponent(this.props.page.section);

            return <div className="vcrnet-faq">
                {(element && element.render()) || null}
            </div>;
        }
    }
}
