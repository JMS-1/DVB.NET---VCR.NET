/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Konfiguration zur Anzeige eines Symbols.
    export interface IPictogram {
        // Der Name einer Symboldatei.
        name: string;

        // Eine alternative Beschreibung für das Symbol.
        description?: string;

        // Aktion bei Auswahl des Symbols.
        onClick?(ev: React.MouseEvent<HTMLImageElement>): void;
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
                onClick={this.props.onClick}
                src={`${Pictogram.imageRoot}${this.props.name}.png`} />;
        }

    }
}
