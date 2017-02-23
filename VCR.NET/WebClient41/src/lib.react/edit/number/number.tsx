﻿/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Konfiguration der Anzeige einer Zahl.
    interface IEditNumber extends IComponent<App.INumber> {
        // Die Anzahl der Zeichen im Texteingabefeld für die Zahl.
        chars: number;
    }

    // React.Js Komponente zur Eingabe einer Zahl über ein Textfeld.
    export class EditNumber extends ComponentExWithSite<App.INumber, IEditNumber>  {

        // Oberflächenelemente erzeugen.
        render(): JSX.Element {
            return <input className="jmslib-editnumber"
                type="TEXT"
                size={this.props.chars}
                value={this.props.uvm.rawValue}
                title={this.props.uvm.message}
                onChange={ev => this.props.uvm.rawValue = (ev.target as HTMLInputElement).value} />;
        }
    }
}
