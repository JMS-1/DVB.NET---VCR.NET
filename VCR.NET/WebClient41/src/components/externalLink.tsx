/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IExternalLinkStatic {
        url: string;
    }

    export class ExternalLink extends React.Component<IExternalLinkStatic, INoDynamicState>{
        render(): JSX.Element {
            return <a className="vcrnet-externalLink" href={this.props.url} target="_blank">{this.props.children}</a>;
        }
    }
}
