/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    export class EditTime extends JMSLib.ReactUi.NoUiViewWithSite<App.NoUi.ITimeEditor> implements JMSLib.App.ISite {
        render(): JSX.Element {
            return <input className="vcrnet-edittime"
                type="TEXT"
                value={this.props.noui.time}
                title={this.props.noui.error}
                size={5}
                onChange={ev => this.props.noui.time = (ev.target as HTMLInputElement).value} />;
        }
    }
}
