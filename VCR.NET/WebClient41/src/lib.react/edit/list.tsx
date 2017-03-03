/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Verwaltete eine Zeichenkette, die über eine einfache Auswahlliste festgelegt wird.
    export class SingleSelect extends ComponentWithSite<App.IValueFromList<any>>  {

        // Erstellt die Anzeige für die Komponente.
        render(): JSX.Element {
            return <select
                title={this.props.uvm.message}
                value={`${this.props.uvm.valueIndex}`}
                className="jmslib-editlist jmslib-validatable"
                onChange={ev => this.props.uvm.valueIndex = parseInt((ev.target as HTMLSelectElement).value)}>
                {this.props.uvm.allowedValues.map((av, index) => <option key={index} value={`${index}`}>{av.display}</option>)}
            </select>;
        }

    }
}
