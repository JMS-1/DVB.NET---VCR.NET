/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {


    // Konfigurationzur Anzeige eines internen Verweises.
    interface IInternalLink {
        // Ein Navigationsziel oder eine Funktion zur Durchführung eines Übergangs.
        view: string | (() => void);

        // Optional ein Symbol für den Verweis.
        pict?: string

        // Optional ein Tooltip.
        description?: string;
    }

    // React.Js Komponente zur Anzeige eines internen Verweises.
    export class InternalLink extends React.Component<IInternalLink, IEmpty>{

        // Erstellt die Oberflächenelemente.
        render(): JSX.Element {
            // Konfiguration des HTML A Verweises je nach Parameter der Komponente.
            var target = "javascript:void(0)";
            var click = undefined;

            if (typeof this.props.view === "function")
                click = this.props.view;
            else
                target = "#" + this.props.view;

            // Verweis mit optionalem Symbol erstellen.
            return <span className="jmslib-internalLink">
                {this.props.pict && <a href={target} title={this.props.description} onClick={click}><Pictogram name={this.props.pict} /></a>}
                {this.props.children && <a href={target} title={this.props.description} onClick={click}>{this.props.children}</a>}
            </span>;
        }

    }
}
