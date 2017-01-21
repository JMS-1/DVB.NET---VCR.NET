/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur visuellen Pflege eines Wahrheitswertes.
    export class EditBoolean extends NoUiView<App.NoUi.IBooleanEditor>  {
        // Erstellt die Anzeige der Komponente.
        render(): JSX.Element {
            return <label className="vcrnet-editflag">
                <input type="CHECKBOX" disabled={this.props.noui.isReadonly} checked={this.props.noui.value} onChange={ev => this.onChange(ev)} />
                {this.props.noui.text}
            </label>;
        }

        // Überträgt einen veränderten Wert in das Modell.
        private onChange(ev: React.FormEvent): any {
            // Veränderten Wert übertragen.
            this.props.noui.value = (ev.target as HTMLInputElement).checked;
        }
    }
}
