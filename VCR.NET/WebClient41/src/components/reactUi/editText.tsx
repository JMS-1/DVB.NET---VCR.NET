/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // Konfiguration einer einfachen Texteingabe.
    interface IEditTextStatic extends INoUiComponent<App.NoUi.IStringEditor> {
        // Die Anzahl der darzustellenden Zeichen.
        chars: number;

        // Optional ein Platzhaltertext.
        hint?: string;
    }

    // Texteingabe für React.Js - die NoUi Schicht stellt den Wert und das Prüfergebnis zur Verfügung.
    export class EditText extends React.Component<IEditTextStatic, INoDynamicState>  {
        // Erstellt die Anzeige der Komponente.
        render(): JSX.Element {
            return <input className="vcrnet-edittext"
                type="TEXT"                
                size={this.props.chars}
                onChange={this._onChange}
                value={this.props.noui.val()}
                placeholder={this.props.hint}
                title={this.props.noui.message} />;
        }

        // Überträgt einen veränderten Wert in die NoUi Schicht, in der dann Prüfungen und eine Aktualisierung ausgelöst werden.
        private readonly _onChange = this.onChange.bind(this);

        private onChange(ev: React.FormEvent): any {
            ev.preventDefault();

            this.props.noui.val((ev.target as HTMLInputElement).value);
        }
    }
}
