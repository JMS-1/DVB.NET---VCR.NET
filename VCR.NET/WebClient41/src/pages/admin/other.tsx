/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminOther extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminOtherPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-other">
                <h2>Sonstige Betriebsparameter</h2>
                <form>
                    <Field label={`${this.props.noui.port.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.port} chars={8} />
                    </Field>
                    <div>
                        <JMSLib.ReactUi.EditBoolean noui={this.props.noui.ssl} />
                    </div>
                    <Field label={`${this.props.noui.securePort.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.securePort} chars={8} />
                    </Field>
                    <div>
                        <JMSLib.ReactUi.EditBoolean noui={this.props.noui.basicAuth} />
                    </div>
                    <Field label={`${this.props.noui.hibernation.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.SelectSingleFromList noui={this.props.noui.hibernation} />
                    </Field>
                    <Field label={`${this.props.noui.preSleep.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.preSleep} chars={8} />
                    </Field>
                    <Field label={`${this.props.noui.minSleep.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.minSleep} chars={8} />
                    </Field>
                    <JMSLib.ReactUi.EditBoolean noui={this.props.noui.ignoreMinSleep} />
                    <Field label={`${this.props.noui.logKeep.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.logKeep} chars={8} />
                    </Field>
                    <Field label={`${this.props.noui.jobKeep.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.EditNumber noui={this.props.noui.jobKeep} chars={8} />
                    </Field>
                    <div>
                        <JMSLib.ReactUi.EditBoolean noui={this.props.noui.noH264PCR} />
                    </div>
                    <div>
                        <JMSLib.ReactUi.EditBoolean noui={this.props.noui.noMPEG2PCR} />
                    </div>
                    <Field label={`${this.props.noui.logging.text}:`} page={this.props.noui.page} >
                        <JMSLib.ReactUi.SelectSingleFromList noui={this.props.noui.logging} />
                    </Field>
                </form>
                <div>
                    <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.update} />
                </div>
            </div>;
        }
    }

}
