/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Verwaltete eine Zeichenkette, die über eine einfache Auswahlliste festgelegt wird.
    export class SelectSingleFromList extends ComponentWithSite<App.IValueFromList<any>>  {
        // Erstellt die Anzeige für die Komponente.
        render(): JSX.Element {
            return <select
                className="jmslib-editlist"
                value={`${this.props.noui.displayValue}`}
                title={this.props.noui.message}
                onChange={ev => this.props.noui.displayValue = (ev.target as HTMLSelectElement).value}>
                {this.props.noui.allowedValues.map(av => <option key={av.display} value={`${av.value}`}>{av.display}</option>)}
            </select>;
        }
    }
}
