/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Konfiguration zur Anzeige eines Symbols.
    interface IPictogram {
        // Der Name einer Symboldatei.
        name: string;

        // Die Art der Symboldatei - ist dieser Parameter nicht gesetzt, so wird PNG angenommen.
        type?: string;

        // Eine alternative Beschreibung für das Symbol.
        description?: string;

        // Aktion bei Auswahl des Symbols.
        onClick?: (ev: React.FormEvent) => void;
    }

    // React.Js Komponente zur Anzeige eines Symbols.
    export class Pictogram extends React.Component<IPictogram, IEmpty>{
        // Globale Festlegung für das Verzeichnis aller Symboldateien.
        static imageRoot: string;

        // Erstellt die Oberflächenelemente.
        render(): JSX.Element {
            return <img
                className="jmslib-pict"
                alt={this.props.description}
                src={`${Pictogram.imageRoot}${this.props.name}.${this.props.type || "png"}`}
                onClick={this.props.onClick} />;
        }
    }
}
