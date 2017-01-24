/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient.Ui {
    interface IHelpLinkStatic {
        page: App.IPage;
          
        topic: string;
    }

    export class HelpLink extends React.Component<IHelpLinkStatic, JMSLib.ReactUi.IEmpty>{
        render(): JSX.Element {
            return <a className="vcrnet-helpLink" href={`#${this.props.page.application.helpPage.route};${this.props.topic}`}><JMSLib.ReactUi.Pictogram name="info" /></a>;
        }
    }
}
