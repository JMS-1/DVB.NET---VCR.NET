/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // Konfiguration einer einfachen Auswahlliste in React.Js.
    interface IEditSelectStatic {
        // Die zugehörige Formular- und Prüflogik.
        noui: App.NoUi.IStringFromListEditor;
    }

    // Der aktuelle Zustand einer einfachen Auswahlliste.
    interface IEditTextDynamic {
        // Die aktuell ausgewählte Variante.
        current: string;
    }

    // Verwaltete eine Zeichenkette, die über eine einfache Auswahlliste festgelegt wird.
    export class EditTextWithList extends React.Component<IEditSelectStatic, IEditTextDynamic>  {
        // Wird bei einer veränderten Auswahl aufgerufen.
        private readonly _onChange = this.onChange.bind(this);

        // Wird einmalig beim Einklinken der Komponente in die Anzeige aufgerufen.
        componentWillMount(): void {
            this.componentWillReceiveProps(this.props, undefined);
        }

        // Teil der Komponente mit, dass sich die Konfiguration verändert hat.
        componentWillReceiveProps(nextProps: IEditSelectStatic, nextContext: any): void {
            // Überträgt den aktuellen Wert in die Auswahl.
            this.setState({ current: nextProps.noui.val() });
        }

        // Erstellt die Anzeige für die Komponente.
        render(): JSX.Element {
            return <select className="vcrnet-editlist" value={this.state.current} onChange={this._onChange}>
                {this.props.noui.allowedValues.map(av => <option key={av.display} value={`${av.value}`}>{av.display}</option>)}
            </select>;
        }

        // Übergibt eine veränderte Auswahl an die NoUi-Schicht.
        private onChange(ev: React.FormEvent): any {
            ev.preventDefault();

            this.props.noui.val((ev.target as HTMLSelectElement).value);
        }
    }
}
