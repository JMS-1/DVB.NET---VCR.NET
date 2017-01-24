/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient.Ui {
    interface IExternalLinkStatic {
        url: string;
    }

    export class ExternalLink extends React.Component<IExternalLinkStatic, JMSLib.ReactUi.IEmpty>{
        render(): JSX.Element {
            return <a className="vcrnet-externalLink" href={this.props.url} target="_blank">{this.props.children}</a>;
        }
    }
}
