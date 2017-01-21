/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // Schnittstelle zur Pflege einer Ausnahmeregel.
    interface IPlanExceptionStatic extends INoUiComponent<App.NoUi.IPlanException> {
        // Die aktuell angezeigte Seite.
        page: App.NoUi.IPage;
    }

    // React.Js Komponente zur Pflege einer einzelnen Ausnahmeregel.
    export class PlanException extends NoUiViewExWithSite<App.NoUi.IPlanException, IPlanExceptionStatic> {
        // Erstellt die Oberflächenelemente zur Pflege.
        render(): JSX.Element {
            return <fieldset className="vcrnet-planexception">
                <legend>Ausnahmeregel bearbeiten<HelpLink page={this.props.page} topic="repeatingschedules" /></legend>
                <table>
                    <tbody>
                        <tr>
                            <td>Start</td>
                            <td>{this.props.noui.currentStart}</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>Ende</td>
                            <td>{this.props.noui.currentEnd}</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>Dauer</td>
                            <td>{`${this.props.noui.currentDuration} Minute${(this.props.noui.currentDuration === 1) ? '' : 'n'}`}</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>Startverschiebung</td>
                            <td>{`${this.props.noui.startSlider.value} Minute${(Math.abs(this.props.noui.startSlider.value) === 1) ? '' : 'n'}`}</td>
                            <td><EditNumberWithSlider noui={this.props.noui.startSlider} /></td>
                        </tr>
                        <tr>
                            <td>Laufzeitanpassung</td>
                            <td>{`${this.props.noui.durationSlider.value} Minute${(Math.abs(this.props.noui.durationSlider.value) === 1) ? '' : 'n'}`}</td>
                            <td><EditNumberWithSlider noui={this.props.noui.durationSlider} /></td>
                        </tr>
                    </tbody>
                </table>
                <div>
                    <button onClick={() => this.props.noui.setToOriginal()}>Ursprüngliche Planung</button>
                    <button onClick={() => this.props.noui.setToDisable()}>Nicht aufzeichnen</button>
                    <button onClick={() => this.props.noui.update()}>Einstellungen übernehmen</button>
                </div>
            </fieldset>;
        }
    }
}