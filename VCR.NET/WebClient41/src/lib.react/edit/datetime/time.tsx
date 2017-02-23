/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class EditTime extends ComponentWithSite<App.ITime> {
        render(): JSX.Element {
            return <input className="jmslib-edittime"
                type="TEXT"
                value={this.props.uvm.rawValue}
                title={this.props.uvm.message}
                size={5}
                onChange={ev => this.props.uvm.rawValue = (ev.target as HTMLInputElement).value} />;
        }
    }
}
