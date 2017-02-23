/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class SingleSelectButton extends ComponentWithSite<App.IValueFromList<any>>  {

        // Erstellt die Anzeige für die Komponente.
        render(): JSX.Element {
            var value = this.props.uvm.value;

            return <div className="jmslib-editbuttonlist">
                {this.props.uvm.allowedValues.map(av => <div
                    title=""
                    key={av.display}
                    className="jmslib-command"
                    onClick={ev => this.props.uvm.value = av.value}
                    data-jmslib-checked={(av.value === value) ? "yes" : "no"}>
                    {av.display}
                </div>)}
            </div>;
        }

    }
}
