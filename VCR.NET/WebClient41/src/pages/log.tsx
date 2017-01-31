/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Log extends JMSLib.ReactUi.ComponentWithSite<App.ILogPage>{
        render(): JSX.Element {
            return <div className="vcrnet-log">
                <form>
                    <Field page={this.props.noui} label={`${this.props.noui.profiles.text}:`}>
                        <JMSLib.ReactUi.EditTextWithList noui={this.props.noui.profiles} />
                        <JMSLib.ReactUi.EditTextWithList noui={this.props.noui.startDay} />
                        <JMSLib.ReactUi.EditBooleanWithButton noui={this.props.noui.showGuide} />
                        <JMSLib.ReactUi.EditBooleanWithButton noui={this.props.noui.showScan} />
                        <JMSLib.ReactUi.EditBooleanWithButton noui={this.props.noui.showLive} />
                    </Field>
                </form>
                <table>
                    <thead>
                        <tr>
                            <td>Beginn</td>
                            <td>Ende</td>
                            <td>Quelle</td>
                        </tr>
                    </thead>
                    <tbody>
                        {this.props.noui.entries.map((e, index) => [
                            <tr key={index}>
                                <td><a href="javascript:void(0)" onClick={() => e.toggleDetail()}>{e.start}</a></td>
                                <td>{e.endTime}</td>
                                <td>{e.source}</td>
                            </tr>,
                            e.showDetail ? <JMSLib.ReactUi.DetailRow key={`${index}Detail`} dataColumns={3}>
                                <LogDetails noui={e} />
                            </JMSLib.ReactUi.DetailRow> : null
                        ])}
                    </tbody>
                </table>
            </div>;
        }
    }

}
