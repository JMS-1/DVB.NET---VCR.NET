/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zum manuellen Starten einer Sonderaufgabe.
    export class Task extends JMSLib.ReactUi.ComponentWithSite<JMSLib.App.ICommand> {

        // Oberflächenelemente anlegen.
        render(): JSX.Element {
            return <li className="vcrnet-home-task">
                <fieldset>
                    {this.props.children}
                    <div><JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm} /></div>
                </fieldset>
            </li>;
        }
    }

}
