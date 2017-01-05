/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IHelpStatic {
        noui: App.HelpPage;

        faqs: IHelpComponentProvider;
    }

    export class Help extends React.Component<IHelpStatic, INoDynamicState> {
        render(): JSX.Element {
            var element = this.props.faqs.getHelpComponent(this.props.noui.section);

            return <div className="vcrnet-faq">
                {(element && element.render()) || null}
            </div>;
        }
    }
}
