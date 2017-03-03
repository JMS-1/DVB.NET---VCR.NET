/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Konfiguration zur Eingabe eines langen Textes.
    interface IEditTextArea extends IComponent<App.IString> {
        // Zeilen zur Eingabe.
        rows: number;

        // Spalten zur Eingabe.
        columns: number;
    }

    // React.Js Komponente zur Eingabe eines langen Textes.
    export class EditTextArea extends ComponentExWithSite<App.IString, IEditTextArea>  {

        // Oberflächenelemente erstellen.
        render(): JSX.Element {
            return <textarea
                rows={this.props.rows}
                cols={this.props.columns}
                value={this.props.uvm.value}
                title={this.props.uvm.message}
                className="jmslib-edittextarea"
                onChange={ev => this.props.uvm.value = (ev.target as HTMLTextAreaElement).value} />;
        }

    }
}
