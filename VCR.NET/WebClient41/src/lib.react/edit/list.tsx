/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Verwaltete eine Zeichenkette, die über eine einfache Auswahlliste festgelegt wird.
    export class SelectSingleFromList extends ComponentWithSite<App.IValueFromList<any>>  {
        // Erstellt die Anzeige für die Komponente.
        render(): JSX.Element {
            return <select
                className="jmslib-editlist"
                value={`${this.props.uvm.valueIndex}`}
                title={this.props.uvm.message}
                onChange={ev => this.props.uvm.valueIndex = parseInt((ev.target as HTMLSelectElement).value)}>
                {this.props.uvm.allowedValues.map((av, index) => <option key={index} value={`${index}`}>{av.display}</option>)}
            </select>;
        }
    }
}
