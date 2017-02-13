/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    interface IPlanGuide extends JMSLib.ReactUi.IComponent<App.Plan.IPlanEntry> {
        page: App.IPage;
    }

    export class PlanGuide extends JMSLib.ReactUi.ComponentExWithSite<App.Plan.IPlanEntry, IPlanGuide> {
        render(): JSX.Element {
            var guide = this.props.noui.guideItem;

            return <div className="vcrnet-planguide">
                <div>
                    <span>Aufzeichnungsoptionen:</span>
                    <HelpLink topic="filecontents" page={this.props.page} />
                    <span className={this.props.noui.allAudio ? undefined : `vcrnet-optionoff`}>Alle Sprachen</span>
                    , <span className={this.props.noui.dolby ? undefined : `vcrnet-optionoff`}>Dolby Digital</span>
                    , <span className={this.props.noui.ttx ? undefined : `vcrnet-optionoff`}>Videotext</span>
                    , <span className={this.props.noui.subs ? undefined : `vcrnet-optionoff`}>DVB-Untertitel</span>
                    , <span className={this.props.noui.guide ? undefined : `vcrnet-optionoff`}>Programminformationen</span>
                </div>
                {guide ?
                    <fieldset>
                        <legend>Auszug aus der Programmzeitschrift</legend>
                        <JMSLib.ReactUi.TimeBar noui={this.props.noui.guideTime} />
                        <GuideEntryInfo noui={guide} />
                    </fieldset> :
                    <div>
                        <span>Programmzeitschrift:</span> <span className="vcrnet-optionoff">wird abgerufen</span>
                    </div>}
            </div>;
        }
    }
}