/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminGuide extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminGuidePage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-guide">
                <h2>Einstellungen zum Aufbau der Programmzeitschrift</h2>
                <div>
                    Auf Wunsch kann der VCR.NET Recording Service die Elektronische Programmzeitschrift
                    (EPG)<HelpLink topic="epg" page={this.props.noui.page} /> periodisch
                    aktualisieren<HelpLink topic="epgconfig" page={this.props.noui.page} /> und
                    dann zur Programmierung von neuen Aufzeichnungen
                    anbieten.<JMSLib.ReactUi.InternalLink view="edit" pict="new" /> Hier
                    werden die Eckdaten für die Aktualisierung festgelegt.
                </div>
                <JMSLib.ReactUi.EditBoolean noui={this.props.noui.isActive} />
                {this.props.noui.isActive.value ? <form>
                    <div>
                        <JMSLib.ReactUi.EditMultiValueList noui={this.props.noui.sources} items={10} />
                        <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.remove} />
                    </div>
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.ukTv} />
                    <Field page={this.props.noui.page} label={`${this.props.noui.device.text}:`}>
                        <JMSLib.ReactUi.EditTextWithList noui={this.props.noui.device} />
                        <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.add} />
                    </Field>
                    <EditChannel noui={this.props.noui.source} />
                    <Field page={this.props.noui.page} label={`${this.props.noui.duration.text}:`} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.duration} chars={5} />
                    </Field>
                    <Field page={this.props.noui.page} label={`${this.props.noui.hours.text}:`} >
                        <JMSLib.ReactUi.EditMultiButtonList noui={this.props.noui.hours} />
                    </Field>
                    <Field page={this.props.noui.page} label={`${this.props.noui.delay.text}:`} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.delay} chars={5} />
                    </Field>
                    <Field page={this.props.noui.page} label={`${this.props.noui.latency.text}:`} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.latency} chars={5} />
                    </Field>
                </form> : null}
            </div>;
        }
    }

}
