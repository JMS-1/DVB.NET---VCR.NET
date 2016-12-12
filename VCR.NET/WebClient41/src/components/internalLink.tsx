/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IInternalLinkStatic {
        text: string;

        view: string;
    }

    interface IInternalLinkDynamic {
    }

    export class InternalLink extends React.Component<IInternalLinkStatic, IInternalLinkDynamic>{
        render(): JSX.Element {
            return <a className="internalLink" href={"#" + this.props.view}>{this.props.text}</a>;
        }
    }
}
