/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // Verwaltete eine Zeichenkette, die über eine einfache Auswahlliste festgelegt wird.
    export class EditTextWithList extends JMSLib.ReactUi.NoUiView<App.NoUi.IStringFromListEditor>  {
        // Erstellt die Anzeige für die Komponente.
        render(): JSX.Element {
            return <select className="vcrnet-editlist" value={this.props.noui.value} onChange={this._onChange}>
                {this.props.noui.allowedValues.map(av => <option key={av.display} value={`${av.value}`}>{av.display}</option>)}
            </select>;
        }

        // Übergibt eine veränderte Auswahl an die NoUi-Schicht.
        private readonly _onChange = this.onChange.bind(this);

        private onChange(ev: React.FormEvent): any {
            this.props.noui.value = (ev.target as HTMLSelectElement).value;
        }
    }
}
