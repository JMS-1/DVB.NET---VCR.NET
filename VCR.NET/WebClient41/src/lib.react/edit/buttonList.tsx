/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class ButtonFromList extends ComponentWithSite<App.IValueFromList<any>>  {
        // Erstellt die Anzeige für die Komponente.
        render(): JSX.Element {
            var value = this.props.uvm.value;

            return <div className="jmslib-editbuttonlist">
                {this.props.uvm.allowedValues.map(av => <div
                    key={av.display}
                    className="jmslib-command"
                    title=""
                    data-jmslib-checked={(av.value === value) ? "yes" : null}
                    onClick={ev => this.props.uvm.value = av.value}>
                    {av.display}
                </div>)}
            </div>;
        }
    }
}
