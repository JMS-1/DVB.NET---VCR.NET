/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // Konfiguration zur Eingabe eines Wahrheitswertes.
    interface IEditBooleanStatic {
        // Die Formular- und Prüflogik zum Wert.
        noui: App.NoUi.IBooleanEditor;

        // Gesetzt, wenn der Wert nicht verändert werden darf.
        disabled?: boolean;
    }

    // Der aktuelle Zustand der Auswahl.
    interface IEditBooleanDynamic {
        // Der aktuell verwendete Wahrheitswert.
        current: boolean;
    }

    // Bietet einen Wahrheitswert zur Pflege über React.Js an.
    export class EditBoolean extends React.Component<IEditBooleanStatic, IEditBooleanDynamic>  {
        // Wird bei Änderungen des Wertes ausgelöst.
        private readonly _onChange = this.onChange.bind(this);

        // Wird beim ersten Einklinken der Komponente in die Seite aufgerufen.
        componentWillMount(): void {
            this.componentWillReceiveProps(this.props, undefined);
        }

        // Wird immer aufgerufen, wenn sich die Konfiguration der Komponente möglicherweise verändert hat.
        componentWillReceiveProps(nextProps: IEditBooleanStatic, nextContext: any): void {
            // Den aktuellen Wert pflegen wir im Zustand.
            this.setState({ current: nextProps.noui.val() });
        }

        // Erstellt die Anzeige der Komponente.
        render(): JSX.Element {
            return <label className="vcrnet-editflag">
                <input type="CHECKBOX" disabled={this.props.disabled} checked={this.state.current} onChange={this._onChange} />
                {this.props.noui.name}
            </label>;
        }

        // Überträgt einen veränderten Wert in die NoUi Schicht, in der dann Prüfungen und eine Aktualisierung ausgelöst werden.
        private onChange(ev: React.FormEvent): any {
            ev.preventDefault();

            this.props.noui.val((ev.target as HTMLInputElement).checked);
        }
    }
}
