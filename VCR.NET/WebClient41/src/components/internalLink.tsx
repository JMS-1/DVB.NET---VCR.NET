/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IInternalLinkStatic {
        text: string;

        view: string | (() => void);

        pict?: string
    }

    interface IInternalLinkDynamic {
    }

    export class InternalLink extends React.Component<IInternalLinkStatic, IInternalLinkDynamic>{
        render(): JSX.Element {
            var target = "javascript:void(0)";
            var click = undefined;

            if (typeof this.props.view === "function")
                click = this.props.view;
            else
                target = "#" + this.props.view;

            return <span>
                {this.props.pict ? <a className="vcrnet-internalLink" href={target} onClick={click}><Pictogram name={this.props.pict} /></a> : null}
                <a className="vcrnet-internalLink" href={target} onClick={click}>{this.props.text}</a>
            </span>;
        }
    }
}
