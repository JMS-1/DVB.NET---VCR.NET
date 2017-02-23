/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export interface ISelectMultipleFromList extends IComponent<App.IMultiValueFromList<any>> {
        items: number;
    }

    export class SelectMultipleFromList extends ComponentExWithSite<App.IMultiValueFromList<any>, ISelectMultipleFromList>  {
        render(): JSX.Element {
            return <select
                className="jmslib-editmultilist"
                multiple={true}
                size={this.props.items}
                value={this.props.uvm.allowedValues.map((v, index) => v.isSelected ? index : -1).filter(i => i >= 0).map(i => `${i}`)}
                onChange={ev => this.onChange(ev)}>
                {this.props.uvm.allowedValues.map((v, index) => <option key={index} value={`${index}`}>{v.display}</option>)}
            </select>;
        }

        private onChange(ev: React.FormEvent): void {
            var options = (ev.currentTarget as HTMLSelectElement).children;
            var values = this.props.uvm.allowedValues;

            for (var i = 0; i < options.length; i++)
                this.props.uvm.allowedValues[i].isSelected = (options[i] as HTMLOptionElement).selected;
        }
    }
}
