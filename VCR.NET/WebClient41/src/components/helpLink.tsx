/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IHelpLinkStatic {
        page: string;
    }

    interface IHelpLinkDynamic {
    }

    export class HelpLink extends React.Component<IHelpLinkStatic, IHelpLinkDynamic>{
        render(): JSX.Element {
            return <a className="helpLink" href={"#" + this.props.page}><img src="ui/images/info.png"></img></a>;
        }
    }
}
