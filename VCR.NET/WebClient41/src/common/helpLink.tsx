/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // Konfiguration für den Verweis auf eine Hilfeseite.
    interface IHelpLinkStatic {
        // Der Navigationsbereich, aus dem der der Aufruf erfolgt.
        page: App.IPage;

        // Der Name der Hilfeseite.
        topic: string;
    }

    // React.Js Komponente zur Anzeige eines Verweis auf eine Hilfeseite.
    export class HelpLink extends React.Component<IHelpLinkStatic, JMSLib.ReactUi.IEmpty>{

        // Oberflächenelemente anlegen.
        render(): JSX.Element {
            return <span className="vcrnet-helpLink" ><JMSLib.ReactUi.InternalLink view={`${this.props.page.application.helpPage.route};${this.props.topic}`} pict="info"/></span>;
        }
    }
}
