/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IHelpLinkStatic {
        page: App.NoUi.IPage;
          
        topic: string;
    }

    export class HelpLink extends React.Component<IHelpLinkStatic, JMSLib.ReactUi.INoDynamicState>{
        render(): JSX.Element {
            return <a className="vcrnet-helpLink" href={`#${this.props.page.application.helpPage.route};${this.props.topic}`}><Pictogram name="info" /></a>;
        }
    }
}
