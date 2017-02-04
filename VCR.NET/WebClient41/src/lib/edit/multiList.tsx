/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class EditMultiValueList extends ComponentWithSite<App.IMultiValueFromList<any>>  {
        render(): JSX.Element {
            return <select className="jmslib-editmultilist" multiple={true} value={this.props.noui.value} onChange={ev => this.onChange(ev)} >
                {this.props.noui.values.map(v => <option key={v.display} value={v.value}>{v.display}</option>)}
            </select>;
        }

        private onChange(ev: React.FormEvent): void {
            var options = (ev.currentTarget as HTMLSelectElement).children;
            var selected: JMSLib.App.IUiValue<string>[] = [];
            var values = this.props.noui.values;

            for (var i = 0; i < options.length; i++)
                if ((options[i] as HTMLOptionElement).selected)
                    selected.push(values[i]);

            this.props.noui.value = selected.map(v => v.value);
        }
    }
}
