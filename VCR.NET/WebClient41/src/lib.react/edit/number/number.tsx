/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Konfiguration der Anzeige einer Zahl.
    interface IEditNumber extends IComponent<App.INumber> {
        // Die Anzahl der Zeichen im Texteingabefeld für die Zahl.
        chars: number;
    }

    // React.Js Komponente zur Eingabe einer Zahl über ein Textfeld.
    export class EditNumber extends ComponentExWithSite<App.INumber, IEditNumber>  {

        // Oberflächenelemente erzeugen.
        render(): JSX.Element {
            return <input
                type="TEXT"
                size={this.props.chars}
                title={this.props.uvm.message}
                value={this.props.uvm.rawValue}
                className="jmslib-editnumber jmslib-validatable"
                onChange={ev => this.props.uvm.rawValue = (ev.target as HTMLInputElement).value} />;
        }

    }
}
