/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // React.Js Komponente für eine Mehrfachauswahl über einzelne Schaltflächen.
    export class MultiSelectButton extends ComponentWithSite<App.IMultiValueFromList<any>>  {

        // Oberflächenelemente erstellen.
        render(): JSX.Element {
            return <div className="jmslib-editmultibuttonlist">
                {this.props.uvm.allowedValues.map(v => <div
                    title=""
                    key={v.display}
                    className="jmslib-command"
                    onClick={ev => v.isSelected = !v.isSelected}
                    data-jmslib-checked={v.isSelected ? "yes" : "no"}>
                    {v.display}
                </div>)}
            </div>;
        }

    }
}
