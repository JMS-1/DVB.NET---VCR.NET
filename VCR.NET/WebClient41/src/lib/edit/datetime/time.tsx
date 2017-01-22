/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class EditTime extends ComponentWithSite<App.IEditTime> {
        render(): JSX.Element {
            return <input className="jmslib-edittime"
                type="TEXT"
                value={this.props.noui.time}
                title={this.props.noui.error}
                size={5}
                onChange={ev => this.props.noui.time = (ev.target as HTMLInputElement).value} />;
        }
    }
}
