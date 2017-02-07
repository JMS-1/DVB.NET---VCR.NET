/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // React.Js Komponente zur visuellen Pflege eines Wahrheitswertes.
    export class EditBoolean extends ComponentWithSite<App.IValidatedFlag>  {
        // Erstellt die Anzeige der Komponente.
        render(): JSX.Element {
            return <label className="jmslib-editflag" title={this.props.noui.message}>
                <input
                    type="CHECKBOX"
                    disabled={this.props.noui.isReadonly}
                    checked={this.props.noui.value}
                    onChange={ev => this.props.noui.value = (ev.target as HTMLInputElement).checked} />
                {this.props.noui.text}
            </label>;
        }
    }
}
