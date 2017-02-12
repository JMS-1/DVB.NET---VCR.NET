/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Verwaltete eine Zeichenkette, die über eine einfache Auswahlliste festgelegt wird.
    export class SelectSingleFromList extends ComponentWithSite<App.IValueFromList<any>>  {
        // Erstellt die Anzeige für die Komponente.
        render(): JSX.Element {
            return <select
                className="jmslib-editlist"
                value={`${this.props.noui.displayValueIndex}`}
                title={this.props.noui.message}
                onChange={ev => this.props.noui.displayValueIndex = parseInt((ev.target as HTMLSelectElement).value)}>
                {this.props.noui.allowedValues.map((av, index) => <option key={index} value={`${index}`}>{av.display}</option>)}
            </select>;
        }
    }
}
