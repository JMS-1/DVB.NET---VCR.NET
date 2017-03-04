/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // Schnittstelle zur Pflege einer Ausnahmeregel.
    interface IPlanExceptionStatic extends JMSLib.ReactUi.IComponent<App.Plan.IPlanException> {
        // Die aktuell angezeigte Seite.
        page: App.IPlanPage;
    }

    // React.Js Komponente zur Pflege einer einzelnen Ausnahmeregel.
    export class PlanException extends JMSLib.ReactUi.ComponentExWithSite<App.Plan.IPlanException, IPlanExceptionStatic> {

        // Erstellt die Oberflächenelemente zur Pflege.
        render(): JSX.Element {
            return <fieldset className="vcrnet-planexception">
                <legend>Ausnahmeregel bearbeiten<HelpLink page={this.props.page} topic="repeatingschedules" /></legend>
                <table className="vcrnet-tableIsForm">
                    <tbody>
                        <tr>
                            <td>Start</td>
                            <td>{this.props.uvm.currentStart}</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>Ende</td>
                            <td>{this.props.uvm.currentEnd}</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td className={(this.props.uvm.currentDuration <= 0) ? `vcrnet-planexception-discard` : undefined}>Dauer</td>
                            <td>{`${this.props.uvm.currentDuration} Minute${(this.props.uvm.currentDuration === 1) ? '' : 'n'}`}</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>Startverschiebung</td>
                            <td>{`${this.props.uvm.startSlider.value} Minute${(Math.abs(this.props.uvm.startSlider.value) === 1) ? '' : 'n'}`}</td>
                            <td><JMSLib.ReactUi.EditNumberSlider uvm={this.props.uvm.startSlider} /></td>
                        </tr>
                        <tr>
                            <td>Laufzeitanpassung</td>
                            <td>{`${this.props.uvm.durationSlider.value} Minute${(Math.abs(this.props.uvm.durationSlider.value) === 1) ? '' : 'n'}`}</td>
                            <td><JMSLib.ReactUi.EditNumberSlider uvm={this.props.uvm.durationSlider} /></td>
                        </tr>
                    </tbody>
                </table>
                <div>
                    <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.originalTime} />
                    <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.skip} />
                    <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.update} />
                </div>
            </fieldset>;
        }
    }
}