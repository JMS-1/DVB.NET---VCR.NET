/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // Konfiguration zur Auswahl mehrerer Elemente aus einer Liste erlaubter Elemente.
    export interface ISelectMultipleFromList extends IComponent<App.IMultiValueFromList<any>> {
        // Die Größe der Liste.
        items: number;
    }

    // React.Js Komponente zur Auswahl von mehreren Elementen aus eine Liste von Elementen.
    export class MultiSelect extends ComponentExWithSite<App.IMultiValueFromList<any>, ISelectMultipleFromList>  {

        // Erstellt die Oberflächenelemente.
        render(): JSX.Element {
            return <select
                multiple={true}
                size={this.props.items}
                className="jmslib-editmultilist"
                onChange={ev => this.onChange(ev)}
                value={this.props.uvm.allowedValues.map((v, index) => v.isSelected ? index : -1).filter(i => i >= 0).map(i => `${i}`)}>
                {this.props.uvm.allowedValues.map((v, index) => <option key={index} value={`${index}`}>{v.display}</option>)}
            </select>;
        }

        // Die Veränderung der Auswahl ist in dieser Implementierung etwas aufwändiger, da wir den Algorithmus des Browsers unverändert übernehmen wollen.
        private onChange(ev: React.FormEvent<HTMLSelectElement>): void {
            // Alle Oberflächenelemente zur Auswahlliste.
            var options = (ev.currentTarget as HTMLSelectElement).children;

            // Die zugehörige Auswahlliste.
            var values = this.props.uvm.allowedValues;

            // Auswahl einfach wie vom Browser gewünscht übertragen.
            for (var i = 0; i < options.length; i++)
                this.props.uvm.allowedValues[i].isSelected = (options[i] as HTMLOptionElement).selected;
        }

    }
}
