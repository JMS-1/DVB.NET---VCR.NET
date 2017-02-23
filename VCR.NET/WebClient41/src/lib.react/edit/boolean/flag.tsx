/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // React.Js Komponente zur visuellen Pflege eines Wahrheitswertes.
    export class EditBoolean extends ComponentWithSite<App.IFlag>  {
        // Erstellt die Anzeige der Komponente.
        render(): JSX.Element {
            return <label className="jmslib-editflag" title={this.props.uvm.message}>
                <input
                    type="CHECKBOX"
                    disabled={this.props.uvm.isReadonly}
                    checked={this.props.uvm.value}
                    onChange={ev => this.props.uvm.value = (ev.target as HTMLInputElement).checked} />
                {this.props.uvm.text}
            </label>;
        }
    }
}
