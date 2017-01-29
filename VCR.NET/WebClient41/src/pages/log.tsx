/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Log extends JMSLib.ReactUi.ComponentWithSite<App.ILogPage>{
        render(): JSX.Element {
            return <div className="vcrnet-log">
                <form>
                    <Field page={this.props.noui} label={`${this.props.noui.profiles.text}:`}>
                        <JMSLib.ReactUi.EditTextWithList noui={this.props.noui.profiles} />
                    </Field>
                </form>
            </div>;
        }
    }

}
