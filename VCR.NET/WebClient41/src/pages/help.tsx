/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IHelpStatic {
        noui: App.HelpPage;

        topics: IHelpComponentProvider;
    }

    export class Help extends React.Component<IHelpStatic, INoDynamicState> {
        render(): JSX.Element {
            var element = this.props.topics.getHelpComponent(this.props.noui.section);

            return <div className="vcrnet-faq">
                {(element && element.render(this.props.noui)) || null}
            </div>;
        }
    }
}
