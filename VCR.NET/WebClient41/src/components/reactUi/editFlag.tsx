/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur visuellen Pflege eines Wahrheitswertes.
    export class EditBoolean extends NoUiView<App.NoUi.IBooleanEditor>  {
        // Erstellt die Anzeige der Komponente.
        render(): JSX.Element {
            return <label className="vcrnet-editflag">
                <input type="CHECKBOX" disabled={this.props.noui.isReadonly()} checked={this.props.noui.val()} onChange={this._onChange} />
                {this.props.noui.text}
            </label>;
        }

        // Überträgt einen veränderten Wert in das Modell.
        private readonly _onChange = this.onChange.bind(this);

        private onChange(ev: React.FormEvent): any {
            // Die Aktualisierung der Oberfläche wird alleinig durch das Modell gesteuert.
            ev.preventDefault();

            // Veränderten Wert übertragen.
            this.props.noui.val((ev.target as HTMLInputElement).checked);
        }
    }
}
