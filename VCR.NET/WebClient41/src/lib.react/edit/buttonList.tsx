/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Schnittstelle zur Anzeige einer Liste von Schaltflächen. 
    interface ISingleSelectButton extends IComponent<App.IValueFromList<any>> {
        // Gesetzt, wenn die Schaltflächen nicht separiert werden sollen.
        merge?: boolean;
    }

    // React.Js Komponente zur Anzeige einer Liste von Schaltflächen.
    export class SingleSelectButton extends ComponentExWithSite<App.IValueFromList<any>, ISingleSelectButton>  {

        // Erstellt die Anzeige für die Komponente.
        render(): JSX.Element {
            var value = this.props.uvm.value;

            return <div className={`jmslib-editbuttonlist${this.props.merge ? ` jmslib-command-mergelist` : ``}`}>
                {this.props.uvm.allowedValues.map(av =>
                    <ButtonCommand uvm={av.select} key={av.display} className={av.isSelected ? `jmslib-command-checked` : ``} />)}
            </div>;
        }

    }
}
