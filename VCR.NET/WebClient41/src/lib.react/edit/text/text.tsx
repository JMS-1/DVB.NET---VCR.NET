/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Konfiguration einer einfachen Texteingabe.
    interface IEditText extends IComponent<App.IString> {
        // Die Anzahl der darzustellenden Zeichen.
        chars: number;

        // Optional ein Platzhaltertext.
        hint?: string;
    }

    // Texteingabe für React.Js - die NoUi Schicht stellt den Wert und das Prüfergebnis zur Verfügung.
    export class EditText extends ComponentExWithSite<App.IString, IEditText>  {

        // Erstellt die Anzeige der Komponente.
        render(): JSX.Element {
            return <input
                type="TEXT"
                size={this.props.chars}
                value={this.props.uvm.value}
                placeholder={this.props.hint}
                title={this.props.uvm.message}
                className="jmslib-edittext jmslib-validatable"
                onChange={ev => this.props.uvm.value = (ev.target as HTMLInputElement).value} />;
        }

    }
}
