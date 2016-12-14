/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IInternalLinkStatic {
        text: string;

        view: string;

        pict?: string
    }

    interface IInternalLinkDynamic {
    }

    export class InternalLink extends React.Component<IInternalLinkStatic, IInternalLinkDynamic>{
        render(): JSX.Element {
            return <span>
                {this.props.pict ? <a className="vcrnet-internalLink" href={"#" + this.props.view}><Pictogram name={this.props.pict} /></a> : null}
                <a className="vcrnet-internalLink" href={"#" + this.props.view}>{this.props.text}</a>
            </span>;
        }
    }
}
