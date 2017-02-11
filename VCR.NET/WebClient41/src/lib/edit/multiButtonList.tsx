/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class MultiButtonsFromList extends ComponentWithSite<App.IMultiValueFromList<any>>  {
        render(): JSX.Element {
            return <div className="jmslib-editmultibuttonlist">
                {this.props.noui.values.map(v => <div
                    key={v.display}
                    className="jmslib-command"
                    title=""
                    data-jmslib-checked={v.selected ? "yes" : null}
                    onClick={ev => v.selected = !v.selected}>
                    {v.display}
                </div>)}
            </div>;
        }
    }
}
