/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // Konfiguration einer einfachen Texteingabe.
    interface IEditTextStatic {
        // Die zugehörige Formular- und Prüflogik.
        noui: App.NoUi.IStringEditor;

        // Die Anzahl der darzustellenden Zeichen.
        chars: number;

        // Optional ein Platzhaltertext.
        hint?: string;
    }

    // Interner Zustand der Texteingabe.
    interface IEditTextDynamic {
        // Der aktuelle Wert.
        current: string;
    }

    // Texteingabe für React.Js - die NoUi Schicht stellt den Wert und das Prüfergebnis zur Verfügung.
    export class EditText extends React.Component<IEditTextStatic, IEditTextDynamic>  {
        // Wird bei Änderungen des Wertes ausgelöst.
        private readonly _onChange = this.onChange.bind(this);

        // Wird beim ersten Einklinken der Komponente in die Seite aufgerufen.
        componentWillMount(): void {
            this.componentWillReceiveProps(this.props, undefined);
        }

        // Wird immer aufgerufen, wenn sich die Konfiguration der Komponente möglicherweise verändert hat.
        componentWillReceiveProps(nextProps: IEditTextStatic, nextContext: any): void {
            // Den aktuellen Wert pflegen wir im Zustand.
            this.setState({ current: nextProps.noui.val() });
        }

        // Erstellt die Anzeige der Komponente.
        render(): JSX.Element {
            return <input className="vcrnet-edittext"
                type="TEXT"                
                size={this.props.chars}
                onChange={this._onChange}
                value={this.state.current}
                placeholder={this.props.hint}
                title={this.props.noui.message} />;
        }

        // Überträgt einen veränderten Wert in die NoUi Schicht, in der dann Prüfungen und eine Aktualisierung ausgelöst werden.
        private onChange(ev: React.FormEvent): any {
            this.props.noui.val((ev.target as HTMLInputElement).value);
        }
    }
}
