/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // Konfiguration zur Anzeige der Details einer Aufzeichnung.
    interface IPlanGuide extends JMSLib.ReactUi.IComponent<App.Plan.IPlanEntry> {
        // Der zugehörige Navigationsbereich.
        page: App.IPlanPage;
    }

    // React.Js Komponente zur Anzeige der Details einer Aufzeichnung.
    export class PlanGuide extends JMSLib.ReactUi.ComponentExWithSite<App.Plan.IPlanEntry, IPlanGuide> {

        // Oberflächenelemente erstellen.
        render(): JSX.Element {
            var guide = this.props.uvm.guideItem;

            return <div className="vcrnet-planguide">
                <div>
                    <span>Aufzeichnungsoptionen:</span>
                    <HelpLink topic="filecontents" page={this.props.page} />
                    <span className={this.props.uvm.allAudio ? undefined : `vcrnet-optionoff`}>Alle Sprachen</span>
                    , <span className={this.props.uvm.dolby ? undefined : `vcrnet-optionoff`}>Dolby Digital</span>
                    , <span className={this.props.uvm.ttx ? undefined : `vcrnet-optionoff`}>Videotext</span>
                    , <span className={this.props.uvm.subs ? undefined : `vcrnet-optionoff`}>DVB-Untertitel</span>
                    , <span className={this.props.uvm.guide ? undefined : `vcrnet-optionoff`}>Programminformationen</span>
                </div>
                {guide ?
                    <fieldset>
                        <legend>Auszug aus der Programmzeitschrift</legend>
                        <JMSLib.ReactUi.TimeBar uvm={this.props.uvm.guideTime} />
                        <GuideEntryInfo uvm={guide} />
                    </fieldset> :
                    <div>
                        <span>Programmzeitschrift:</span> <span className="vcrnet-optionoff">wird abgerufen</span>
                    </div>}
            </div>;
        }
    }
}