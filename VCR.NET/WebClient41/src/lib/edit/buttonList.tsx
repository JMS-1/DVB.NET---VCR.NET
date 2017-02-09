/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class EditWithButtonList extends ComponentWithSite<App.IValueFromList<any>>  {
        // Erstellt die Anzeige für die Komponente.
        render(): JSX.Element {
            var value = this.props.noui.value;

            return <div className="jmslib-editlist">
                {this.props.noui.allowedValues.map(av => <button key={av.display} data-jmslib-checked={(av.value === value) ? "yes" : null} onClick={ev => this.applyValue(ev, av.value)}>{av.display}</button>)}
            </div>;
        }

        private applyValue(ev: React.FormEvent, value: any): void {
            ev.preventDefault();

            this.props.noui.value = value;
        }
    }
}
