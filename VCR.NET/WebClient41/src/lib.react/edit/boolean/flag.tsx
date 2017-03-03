/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // React.Js Komponente zur visuellen Pflege eines Wahrheitswertes.
    export class EditBoolean extends ComponentWithSite<App.IFlag>  {

        // Erstellt die Anzeige der Komponente.
        render(): JSX.Element {
            return <label className="jmslib-editflag jmslib-validatable" title={this.props.uvm.message}>
                <input
                    type="CHECKBOX"
                    checked={this.props.uvm.value}
                    disabled={this.props.uvm.isReadonly}
                    onChange={ev => this.props.uvm.value = (ev.target as HTMLInputElement).checked} />
                {this.props.uvm.text}
            </label>;
        }

    }
}
