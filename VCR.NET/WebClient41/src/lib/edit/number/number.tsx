/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    interface IEditNumber extends IComponent<App.IValidatedNumber> {
        chars: number;
    }

    export class EditNumber extends ComponentExWithSite<App.IValidatedNumber, IEditNumber>  {
        render(): JSX.Element {
            return <input className="jmslib-editnumber"
                type="TEXT"
                size={this.props.chars}
                value={this.props.noui.rawValue}
                title={this.props.noui.message}
                onChange={ev => this.props.noui.rawValue = (ev.target as HTMLInputElement).value} />;
        }
    }
}
