/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    interface IEditExceptionStatic extends INoUiComponent<App.NoUi.IPlanException> {
        page: App.NoUi.IPage;
    }

    export class EditException extends NoUiViewExWithSite<App.NoUi.IPlanException, IEditExceptionStatic> {
        render(): JSX.Element {
            return <fieldset className="vcrnet-editexception">
                <legend>Ausnahmeregel bearbeiten <HelpLink page={this.props.page} topic="repeatingschedules" /></legend>
                <table>
                    <tbody>
                        <tr>
                            <td>Start</td>
                            <td>{this.props.noui.getStart()}</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>Ende</td>
                            <td>...</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>Dauer</td>
                            <td>...</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>Startverschiebung</td>
                            <td>... Minute(n)</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>Laufzeitanpassung</td>
                            <td>... Minute(n)</td>
                            <td>&nbsp;</td>
                        </tr>
                    </tbody>
                </table>
                <div>
                    <button>Ursprüngliche Planung</button>
                    <button>Nicht aufzeichnen</button>
                    <button>Einstellungen übernehmen</button>
                </div>
            </fieldset>;
        }
    }
}