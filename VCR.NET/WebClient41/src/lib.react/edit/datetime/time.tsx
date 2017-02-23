/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class EditTime extends ComponentWithSite<App.ITime> {
        render(): JSX.Element {
            return <input className="jmslib-edittime"
                type="TEXT"
                value={this.props.noui.rawValue}
                title={this.props.noui.message}
                size={5}
                onChange={ev => this.props.noui.rawValue = (ev.target as HTMLInputElement).value} />;
        }
    }
}
