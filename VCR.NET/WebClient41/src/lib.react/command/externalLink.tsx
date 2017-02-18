/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Konfiguration eines externen Verweises.
    interface IExternalLink {
        url: string;

        sameWindow?: boolean;
    }

    // React.Js Komponente zur Anzeige eines externen Verweises.
    export class ExternalLink extends React.Component<IExternalLink, IEmpty>{
        // Erstellt die Oberflächenelemente.
        render(): JSX.Element {
            return <a
                className="jmslib-externalLink"
                href={this.props.url}
                target={this.props.sameWindow ? undefined : "_blank"}>{this.props.children}</a>;
        }
    }

}
