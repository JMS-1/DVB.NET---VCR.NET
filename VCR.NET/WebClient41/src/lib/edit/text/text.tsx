/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Konfiguration einer einfachen Texteingabe.
    interface IEditText extends IComponent<App.IValidatedString> {
        // Die Anzahl der darzustellenden Zeichen.
        chars: number;

        // Optional ein Platzhaltertext.
        hint?: string;
    }

    // Texteingabe für React.Js - die NoUi Schicht stellt den Wert und das Prüfergebnis zur Verfügung.
    export class EditText extends ComponentExWithSite<App.IValidatedString, IEditText>  {
        // Erstellt die Anzeige der Komponente.
        render(): JSX.Element {
            return <input className="jmslib-edittext"
                type="TEXT"
                size={this.props.chars}
                value={this.props.noui.value}
                placeholder={this.props.hint}
                title={this.props.noui.message}
                onChange={ev => this.props.noui.value = (ev.target as HTMLInputElement).value} />;
        }
    }
}
