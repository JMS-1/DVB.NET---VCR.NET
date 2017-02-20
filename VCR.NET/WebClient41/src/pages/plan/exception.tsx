/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // Schnittstelle zur Pflege einer Ausnahmeregel.
    interface IPlanExceptionStatic extends JMSLib.ReactUi.IComponent<App.Plan.IPlanException> {
        // Die aktuell angezeigte Seite.
        page: App.IPage;
    }

    // React.Js Komponente zur Pflege einer einzelnen Ausnahmeregel.
    export class PlanException extends JMSLib.ReactUi.ComponentExWithSite<App.Plan.IPlanException, IPlanExceptionStatic> {
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
                            <td className={(this.props.noui.currentDuration <= 0) ? `vcrnet-planexception-discard` : undefined}>Dauer</td>
                            <td>{`${this.props.noui.currentDuration} Minute${(this.props.noui.currentDuration === 1) ? '' : 'n'}`}</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>Startverschiebung</td>
                            <td>{`${this.props.noui.startSlider.value} Minute${(Math.abs(this.props.noui.startSlider.value) === 1) ? '' : 'n'}`}</td>
                            <td><JMSLib.ReactUi.EditNumberWithSlider noui={this.props.noui.startSlider} /></td>
                        </tr>
                        <tr>
                            <td>Laufzeitanpassung</td>
                            <td>{`${this.props.noui.durationSlider.value} Minute${(Math.abs(this.props.noui.durationSlider.value) === 1) ? '' : 'n'}`}</td>
                            <td><JMSLib.ReactUi.EditNumberWithSlider noui={this.props.noui.durationSlider} /></td>
                        </tr>
                    </tbody>
                </table>
                <div>
                    <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.originalTime} />
                    <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.skip} />
                    <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.update} />
                </div>
            </fieldset>;
        }
    }
}