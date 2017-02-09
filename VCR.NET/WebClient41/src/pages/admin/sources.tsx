/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminSources extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminScanPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-sources">
                <h2>Aktualisierung der Quellen konfigurieren</h2>
                <JMSLib.ReactUi.EditTextWithList noui={this.props.noui.mode} />
                {this.props.noui.showConfiguration ? <form>
                    <Field page={this.props.noui.page} label={`${this.props.noui.duration.text}:`} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.duration} chars={5} />
                    </Field>
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.merge} />
                    {this.props.noui.configureAutomatic ? <div>
                        <Field page={this.props.noui.page} label={`${this.props.noui.hours.text}:`} >
                            <JMSLib.ReactUi.EditMultiButtonList noui={this.props.noui.hours} />
                        </Field>
                        <Field page={this.props.noui.page} label={`${this.props.noui.gapDays.text}:`} >
                            <JMSLib.ReactUi.EditNumber noui={this.props.noui.gapDays} chars={5} />
                        </Field>
                        <Field page={this.props.noui.page} label={`${this.props.noui.latency.text}:`} >
                            <JMSLib.ReactUi.EditNumber noui={this.props.noui.latency} chars={5} />
                        </Field>
                    </div> : null}
                </form> : null}
            </div>;
        }
    }

}
