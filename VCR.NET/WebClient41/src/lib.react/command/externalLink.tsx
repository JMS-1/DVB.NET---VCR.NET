/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Konfiguration eines externen Verweises.
    interface IExternalLink {
        // Der volle Verweis.
        url: string;

        // Gesetzt, wenn eine Anzeige im aktuellen Fenster erfolgen soll - bevorzugt erfolgt die Darstellung in einem neuen Fenster.
        sameWindow?: boolean;
    }

    // React.Js Komponente zur Anzeige eines externen Verweises.
    export class ExternalLink extends React.Component<IExternalLink, IEmpty>{

        // Erstellt die Oberflächenelemente.
        render(): JSX.Element {
            return <a
                target={this.props.sameWindow ? undefined : "_blank"}
                className="jmslib-externalLink"
                href={this.props.url}>{this.props.children}</a>;
        }

    }

}
