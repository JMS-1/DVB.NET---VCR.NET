/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class MultiButtonsFromList extends ComponentWithSite<App.IMultiValueFromList<any>>  {
        render(): JSX.Element {
            return <div className="jmslib-editmultibuttonlist">
                {this.props.noui.allowedValues.map(v => <div
                    key={v.display}
                    className="jmslib-command"
                    title=""
                    data-jmslib-checked={v.isSelected ? "yes" : null}
                    onClick={ev => v.isSelected = !v.isSelected}>
                    {v.display}
                </div>)}
            </div>;
        }
    }
}
