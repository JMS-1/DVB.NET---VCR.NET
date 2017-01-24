/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {
    interface IExternalLink {
        url: string;
    }

    export class ExternalLink extends React.Component<IExternalLink, IEmpty>{
        render(): JSX.Element {
            return <a className="jmslib-externalLink" href={this.props.url} target="_blank">{this.props.children}</a>;
        }
    }
}
