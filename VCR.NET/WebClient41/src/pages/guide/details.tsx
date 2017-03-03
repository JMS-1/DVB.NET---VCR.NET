/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // Konfiguration zur Anzeige der Details zu einer Sendung der Programmzeitschrift.
    interface IGuideDetails extends JMSLib.ReactUi.IComponent<App.Guide.IGuideEntry> {
        // Der zugehörige Navigationsbereich.
        page: App.IGuidePage;
    }

    // React.Js Komponente zur Anzeige der Details einer Sendung der Programmzeitschrift.
    export class GuideDetails extends JMSLib.ReactUi.ComponentEx<App.Guide.IGuideEntry, IGuideDetails> {

        // Oberflächenelemente anlegen.
        render(): JSX.Element {
            return <form className="vcrnet-guideentrydetails">
                <fieldset>
                    <GuideEntryInfo uvm={this.props.uvm} />
                </fieldset>
                <div>
                    <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.createNew} />
                    <Field page={this.props.page} label={`(${this.props.uvm.jobSelector.text}:`}><JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.jobSelector} />)</Field>
                </div>
            </form>;
        }
    }

}
