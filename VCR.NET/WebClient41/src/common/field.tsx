namespace VCRNETClient.Ui {

    // Die Konfiguration eines Eingabefeldes.
    interface IFieldStatic {
        // Die übergeordnete Seite.
        page: App.IPage;

        // Der Anzeigename des Feldes.
        label: string;

        // Optional der Verweise auf eine Hilfeseite.
        help?: string;
    }

    // Beschreibt ein Eingabefeld.
    export class Field extends React.Component<IFieldStatic, JMSLib.ReactUi.IEmpty>{
        // Erzeugt die Anzeige eines Eingabefeldes.
        render(): JSX.Element {
            return <div className="vcrnet-editfield">
                <div>{this.props.label}{this.props.help && <HelpLink page={this.props.page} topic={this.props.help} />}</div>
                <div>{this.props.children}</div>
            </div>;
        }
    }
}
