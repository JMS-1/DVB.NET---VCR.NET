/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // Die Konfiguration eines Eingabefeldes.
    interface IFieldStatic {
        // Der Anzeigename des Feldes.
        label: string;

        // Optional der Verweise auf eine Hilfeseite.
        help?: string;
    }

    // Ein Eingabefeld verwendet keinnen internen Zustand.
    interface IFieldDynamic {
    }

    // Beschreibt ein Eingabefeld.
    export class Field extends React.Component<IFieldStatic, IFieldDynamic>{
        // Erzeugt die Anzeige eines Eingabefeldes.
        render(): JSX.Element {
            return <div className="vcrnet-editfield">
                <div>{this.props.label}{this.props.help ? <HelpLink page={this.props.help} /> : null}</div>
                <div>{this.props.children}</div>
            </div>;
        }
    }
}
