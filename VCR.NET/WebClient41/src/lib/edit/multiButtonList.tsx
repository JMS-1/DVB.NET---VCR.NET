/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class MultiButtonsFromList extends ComponentWithSite<App.IMultiValueFromList<any>>  {
        render(): JSX.Element {
            return <div className="jmslib-editmultibuttonlist">
                {this.props.noui.values.map(v => <button key={v.display} data-jmslib-checked={v.selected ? "yes" : null} onClick={ev => this.onClick(ev, v)} >{v.display}</button>)}
            </div>;
        }

        private onClick(ev: React.FormEvent, value: App.ISelectableUiValue<any>): void {
            ev.preventDefault();

            value.selected = !value.selected;
        }
    }
}
