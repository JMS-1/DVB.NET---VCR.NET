/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IExternalLinkStatic {
        url: string;
    }

    interface IExternalLinkDynamic {
    }

    export class ExternalLink extends React.Component<IExternalLinkStatic, IExternalLinkDynamic>{
        render(): JSX.Element {
            return <a className="vcrnet-externalLink" href={this.props.url} target="_blank">{this.props.children}</a>;
        }
    }
}
