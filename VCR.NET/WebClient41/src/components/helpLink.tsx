/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IHelpLinkStatic {
        page: string;
    }

    export class HelpLink extends React.Component<IHelpLinkStatic, INoDynamicState>{
        render(): JSX.Element {
            return <a className="vcrnet-helpLink" href={"#" + this.props.page}><Pictogram name="info" /></a>;
        }
    }
}
