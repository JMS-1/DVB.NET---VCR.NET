/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // React.Js Komponente zur Eingabe einer Uhrzeit.
    export class EditTime extends ComponentWithSite<App.ITime> {

        // Erstellt die Oberflächenelemente.
        render(): JSX.Element {
            return <input
                size={5}
                type="TEXT"
                title={this.props.uvm.message}
                value={this.props.uvm.rawValue}
                className="jmslib-edittime jmslib-validatable"
                onChange={ev => this.props.uvm.rawValue = (ev.target as HTMLInputElement).value} />;
        }

    }
}
