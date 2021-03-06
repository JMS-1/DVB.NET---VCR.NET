﻿/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Schnittstelle zur Anzeige einer Liste von Schaltflächen. 
    interface IMultiSelectButton extends IComponent<App.IMultiValueFromList<any>> {
        // Gesetzt, wenn die Schaltflächen nicht separiert werden sollen.
        merge?: boolean;
    }

    // React.Js Komponente für eine Mehrfachauswahl über einzelne Schaltflächen.
    export class MultiSelectButton extends ComponentExWithSite<App.IMultiValueFromList<any>, IMultiSelectButton>  {

        // Oberflächenelemente erstellen.
        render(): JSX.Element {
            return <div className={`jmslib-editmultibuttonlist${this.props.merge ? ` jmslib-command-mergelist` : ``}`}>
                {this.props.uvm.allowedValues.map(v =>
                    <ButtonCommand uvm={v.toggle} key={v.display} className={v.isSelected ? `jmslib-command-checked` : ``} />)}
            </div>;
        }

    }
}
